# CUI 엔진 구현계획서

## 개요

GameCore(순수 C#)를 그대로 참조하는 콘솔 애플리케이션.
Unity 없이 터미널에서 검강화 게임을 플레이한다.

```
UnitySword/
├── Assets/Scripts/GameCore/   ← 공유 (수정 없음)
└── CuiApp/                    ← 새로 생성
    ├── CuiApp.sln
    ├── CuiApp/
    │   ├── CuiApp.csproj
    │   ├── Program.cs
    │   ├── Providers/
    │   │   ├── ConsoleRandomProvider.cs
    │   │   └── ConsoleTimeProvider.cs
    │   ├── Repositories/
    │   │   └── JsonFileRepository.cs
    │   ├── Screens/
    │   │   ├── IScreen.cs
    │   │   ├── ScreenManager.cs
    │   │   ├── TitleScreen.cs
    │   │   ├── EnhanceScreen.cs
    │   │   └── WorkshopScreen.cs
    │   └── Rendering/
    │       ├── ConsoleRenderer.cs
    │       ├── EventRenderer.cs
    │       └── SwordAsciiArt.cs
    └── CuiApp.Tests/
        ├── CuiApp.Tests.csproj
        └── SmokeTests.cs
```

---

## 핵심 원칙

1. **GameCore 무수정** — CuiApp은 GameCore 소스를 참조만 한다. Logic/Command/Event/Engine 전부 그대로 사용
2. **Unity 뷰와 동일한 통신 경로** — `engine.Dispatch(Command)` → Event 수신 → 화면 갱신
3. **Provider 교체만으로 동작** — RandomProvider, TimeProvider, Repository를 콘솔용으로 구현
4. **단일 스레드 동기 루프** — 입력 대기 → 커맨드 실행 → 이벤트 연출 → 화면 재렌더

---

## 프로젝트 설정

### CuiApp.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <!-- GameCore 소스를 직접 참조 (dll이 아닌 소스 공유) -->
    <Compile Include="..\..\Assets\Scripts\GameCore\**\*.cs" LinkBase="GameCore" />
  </ItemGroup>
  <ItemGroup>
    <Content Include="..\..\doc\swords.csv" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="..\..\doc\mastery_levels.csv" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
```

### internal 접근에 대한 주의

GameCore 소스를 `<Compile Include>`로 포함하면 **CuiApp과 같은 어셈블리로 컴파일**된다.
이는 Unity 구조(별도 Assembly Definition)와 다르게 `internal` Logic 클래스에 직접 접근 가능하다는 의미.

**CuiApp에서 지켜야 할 규칙:**
- Screen/Rendering 코드에서 `GameCore.Logic.*` 네임스페이스를 **절대 import하지 않는다**
- 반드시 `engine.Dispatch(Command)` 경로만 사용한다
- Unity와 동일한 통신 규약을 **컨벤션으로 강제** (컴파일 타임 강제는 불가)
- 코드 리뷰 시 `using GameCore.Logic` 존재 여부를 체크 포인트로 둔다

> Unity에서는 Assembly Definition의 `internal`이 컴파일 타임에 강제하지만,
> CuiApp에서는 개발자 규율로 대체한다. 이것은 의도된 트레이드오프.

---

## Provider 구현

### ConsoleRandomProvider.cs

```csharp
public class ConsoleRandomProvider : IRandomProvider
{
    private readonly Random _rng;
    public int Seed { get; }

    public ConsoleRandomProvider(int? seed = null)
    {
        Seed = seed ?? Environment.TickCount;
        _rng = new Random(Seed);
    }

    public double NextDouble() => _rng.NextDouble();
}
```

### ConsoleTimeProvider.cs

```csharp
public class ConsoleTimeProvider : ITimeProvider
{
    public DateTime Now() => DateTime.Now;
}
```

### JsonFileRepository.cs

```csharp
public class JsonFileRepository : IStorageRepository
{
    private const string SavePath = "save.json";

    public Task<PlayerData> LoadAsync()
    {
        if (!File.Exists(SavePath))
            return Task.FromResult(PlayerData.CreateDefault());
        var json = File.ReadAllText(SavePath);
        return Task.FromResult(JsonSerializer.Deserialize<PlayerData>(json));
    }

    public Task SaveAsync(PlayerData data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SavePath, json);
        return Task.CompletedTask;
    }
}
```

---

## 화면 구조

### IScreen 인터페이스

```csharp
public interface IScreen
{
    void Render(GameState state, GameContext context);
    /// 입력 처리 후 전환할 화면 이름을 반환. null이면 현재 화면 유지.
    string HandleInput(char key, GameEngine engine);
}
```

- Unity의 View = MonoBehaviour + 이벤트 구독
- CUI의 Screen = **Render(그리기) + HandleInput(입력→커맨드+전환)**
- `HandleInput`이 `string`을 반환하여 화면 전환을 ScreenManager에 알린다
- 입력은 `char` 단일 키 (ReadKey, Enter 불필요)

### 화면 전환 메커니즘

```
TitleScreen → EnhanceScreen ↔ WorkshopScreen
                             ↔ AchievementScreen (P3)
```

```csharp
public class ScreenManager
{
    private readonly Dictionary<string, IScreen> _screens;
    private readonly ConsoleRenderer _renderer;
    private IScreen _current;
    private bool _inputLocked;  // 연출 중 입력 차단

    public void SwitchTo(string name) => _current = _screens[name];

    public void Render(GameState state, GameContext ctx)
        => _current.Render(state, ctx);

    public void HandleInput(char key, GameEngine engine)
    {
        if (_inputLocked) return;  // 연출 중 입력 무시

        var nextScreen = _current.HandleInput(key, engine);
        if (nextScreen != null)
            SwitchTo(nextScreen);
    }

    public void LockInput() => _inputLocked = true;
    public void UnlockInput() => _inputLocked = false;
}
```

**전환 규칙:**
- 화면 전환 키(`W`, `B`)는 커맨드를 디스패치하지 않고 화면 이름만 반환
- 커맨드 키(`E`, `S`, `C`)는 `engine.Dispatch()` 호출 후 null 반환 (화면 유지)
- 연출 재생 중(`_inputLocked = true`) 모든 입력 무시 — Unity의 `IsAnimating` 대응

---

## 메인 게임 루프

### 렌더-이벤트 사이클 문제와 해결

Unity에서는 이벤트 발생 → 비동기 애니메이션 재생 → 완료 후 UI 갱신이 자연스럽다.
CUI에서는 `engine.Dispatch()` 도중 이벤트가 동기 발생하고, 직후 `Console.Clear()`가 호출되면
이벤트 연출(Flash, 텍스트)이 즉시 지워진다.

**해결: 2단계 렌더링**

```
[화면 렌더] → [입력 대기] → [Dispatch] → [이벤트 큐잉] → [이벤트 연출] → [화면 렌더] → ...
```

- 이벤트를 직접 출력하지 않고 **큐에 쌓아둔다**
- Dispatch 완료 후 큐를 꺼내서 **연출 재생** (Flash, Pause, WaitKey)
- 연출 완료 후 `Console.Clear()` → 새 상태로 화면 렌더

### Program.cs

```csharp
// --- 초기화 ---
var random = new ConsoleRandomProvider();
var time = new ConsoleTimeProvider();
var repo = new JsonFileRepository();
var swordTable = SwordDataLoader.Load("swords.csv");
var masteryTable = MasteryDataLoader.Load("mastery_levels.csv");
var context = new GameContext(random, time, swordTable, masteryTable, isAdSystemEnabled: false);
var playerData = repo.Load();  // 동기 I/O — 콘솔이므로 async 불필요
var initialState = GameSessionLogic.CreateInitialState(playerData, swordTable);
// ↑ 내부적으로 IsFirstRun 판단 → 첫 실행 시 200G 지급, 기존 유저는 저장된 골드 유지
// ↑ 현재 검은 항상 나무검(+0)으로 시작 (세션 리셋 규칙)

var engine = new GameEngine(initialState, context);
var renderer = new ConsoleRenderer();
var eventRenderer = new EventRenderer(renderer);
var screens = new ScreenManager(renderer);

// 이벤트를 큐에 쌓기 (직접 출력하지 않음)
engine.OnEvent += evt => eventRenderer.Enqueue(evt);

// --- 게임 루프 ---
screens.SwitchTo("title");

while (true)
{
    // 1. 화면 렌더
    Console.Clear();
    screens.Render(engine.State, context);

    // 2. 입력 (단일 키 — Enter 불필요)
    var key = Console.ReadKey(intercept: true).KeyChar;
    if (key == 'q' || key == 'Q') break;

    // 3. 커맨드 디스패치 (내부에서 이벤트가 큐에 쌓임)
    screens.HandleInput(key, engine);

    // 4. 이벤트 연출 재생 (큐에서 꺼내서 순차 출력)
    eventRenderer.PlayAll();

    // 5. 자동 저장 (판매/수집/파괴 이벤트 발생 시)
    if (eventRenderer.ShouldSave)
        repo.Save(engine.State.PlayerData);
}

// 종료 시 최종 저장
repo.Save(engine.State.PlayerData);
```

### 자동 저장 타이밍

Unity와 동일하게, 결과가 확정되는 시점에 저장한다:

| 트리거 이벤트 | 저장 |
|-------------|------|
| `SellEvent` | O |
| `CollectEvent` | O |
| `EnhanceFailEvent(destroyed: true)` | O |
| `EnhanceSuccessEvent` | X (강화 연타 중 저장 안 함) |
| 종료 (`Q` 키) | O (항상) |

`EventRenderer.ShouldSave`가 위 이벤트 발생 시 true를 반환.

---

## 화면 레이아웃

### TitleScreen

```
╔══════════════════════════════════╗
║         ⚔  검 강 화  ⚔          ║
║                                  ║
║     [ S ] 게임 시작              ║
║     [ Q ] 종료                   ║
╚══════════════════════════════════╝
```

### EnhanceScreen — 상태별 표시 분기

EnhanceScreen은 `GameState`를 읽어 5가지 상태를 구분한다:

#### 상태 1: 나무검 (+0) — 초기/리셋 직후

```
┌─────────────────────────────────┐
│  $ 200 G              Lv.1 장인 │
├─────────────────────────────────┤
│              |                  │
│              |                  │
│             ===                 │
│              |                  │
│                                 │
│        나무검 +0                │
│     "강화를 시작하세요!"         │
├─────────────────────────────────┤
│  [E] 강화 (95% / 5G)           │
│  [W] 공방    [Q] 종료           │
└─────────────────────────────────┘
```
- 판매 버튼 숨김 (나무검은 판매 불가)
- 수집 버튼 숨김

#### 상태 2: 일반 (+1 ~ +9)

```
┌─────────────────────────────────┐
│  $ 1,250 G            Lv.3 장인 │
├─────────────────────────────────┤
│            /|\                  │
│           / | \                 │
│          /  |  \                │
│         /   |   \               │
│        /____|____\              │
│            |||                  │
│                                 │
│        룬소드 +8                │
├─────────────────────────────────┤
│  [E] 강화 (53% / 120G)         │
│  [S] 판매 (295G)               │
│  [W] 공방    [Q] 종료           │
└─────────────────────────────────┘
```

#### 상태 3: 수집 가능 (+10 이상)

```
│  [E] 강화 (??? / 270G)          │
│  [S] 판매 (850G)  [C] 수집      │
│  [W] 공방    [Q] 종료            │
```
- +15 이상: 확률 "???" 표시

#### 상태 4: 골드 부족 (나무검 + 골드 < 강화비용)

```
│  [E] 강화 (95% / 5G) ← 골드 부족│
│  [F] 긴급 지원금 (200G)         │
│  [W] 공방    [Q] 종료           │
```
- 강화 키 입력 시 reject (`insufficient_gold`)
- `[F]`는 이 조건에서만 표시

#### 상태 5: 아이템 장착 중 (P2)

```
│  [부적 ON] [주문서 +5%]          │
│  [E] 강화 (??? / 270G)          │
```
- ActiveModifiers/HasActiveProtection이 있으면 상단에 표시

### WorkshopScreen

```
┌─────────────────────────────────┐
│  === 공 방 ===                  │
├─────────────────────────────────┤
│  [장인 숙련도]                   │
│  Lv.3 (150/400) ████░░░░ 37%   │
│  보상: 강화비용 10% 할인         │
├─────────────────────────────────┤
│  [파편] 12개                    │
│  1. 보호의 부적 (30파편) x0     │
│  2. 축복의 주문서 (15파편) x0   │
│  3. 골드 주머니 (10파편) x1     │
├─────────────────────────────────┤
│  [컬렉션] 2/11 (18%)           │
│  ✦엑스칼리버 ★  □그람  □천총운검│
│  □쿠사나기  □듀랑달  ...        │
├─────────────────────────────────┤
│  [B] 돌아가기                   │
└─────────────────────────────────┘
```

---

## 이벤트 렌더링 (EventRenderer)

Unity 뷰가 이벤트를 받아 애니메이션을 재생하듯, CUI는 이벤트를 큐에 쌓고 Dispatch 완료 후 순차 재생한다.

### EventRenderer

```csharp
public class EventRenderer
{
    private readonly Queue<GameEvent> _queue = new();
    private readonly ConsoleRenderer _r;
    public bool ShouldSave { get; private set; }

    public void Enqueue(GameEvent evt) => _queue.Enqueue(evt);

    /// 큐의 모든 이벤트를 순차 연출. 완료 후 큐 비움.
    public void PlayAll()
    {
        ShouldSave = false;
        while (_queue.Count > 0)
        {
            var evt = _queue.Dequeue();
            Render(evt);
        }
    }

    private void Render(GameEvent evt)
    {
        switch (evt)
        {
            // --- 강화 ---
            case EnhanceSuccessEvent e:
                _r.Flash(ConsoleColor.Green);
                _r.PrintCenter($"★ 강화 성공! +{e.NewLevel} {e.SwordName} ★");
                // 고단계일수록 서스펜스 딜레이 증가
                _r.Pause(e.NewLevel >= 10 ? 1200 : 800);
                break;

            case EnhanceFailEvent e when e.Destroyed:
                _r.Flash(ConsoleColor.Red);
                _r.PrintCenter($"✖ {e.SwordName} +{e.Level} 파괴!");
                // 도발 메시지: 파괴된 검(현재 레벨)의 SellPrice 참조
                _r.PrintCenter($"\"{e.SellPrice}G짜리 검이 가루가 됐다...\"");
                _r.WaitKey();
                ShouldSave = true;
                break;

            case EnhanceFailEvent e when !e.Destroyed:
                // 보호의 부적 발동 — 파괴 대신 단계 유지
                _r.Flash(ConsoleColor.Cyan);
                _r.PrintCenter("부적이 빛나며 검을 보호했다!");
                _r.Pause(800);
                break;

            // --- 경제 ---
            case SellEvent e:
                _r.Flash(ConsoleColor.Yellow);
                _r.PrintCenter($"판매 완료! +{e.Gold}G");
                _r.Pause(600);
                ShouldSave = true;
                break;

            case GoldChangeEvent e when e.Reason == "emergency_fund":
                _r.PrintCenter($"긴급 지원금 +{e.Amount}G!", ConsoleColor.Yellow);
                _r.Pause(500);
                break;

            // --- 수집 ---
            case CollectEvent e:
                _r.Flash(ConsoleColor.Magenta);
                _r.PrintCenter($"수집 완료! {e.SwordName} → 컬렉션 등록");
                _r.Pause(800);
                ShouldSave = true;
                break;

            // --- 파편 (P2) ---
            case FragmentGainEvent e:
                _r.PrintCenter($"파편 +{e.Amount}개 획득", ConsoleColor.DarkCyan);
                _r.Pause(400);
                break;

            case ExchangeEvent e:
                _r.PrintCenter($"{e.ItemName} 교환 완료!", ConsoleColor.Cyan);
                _r.Pause(500);
                break;

            case UseItemEvent e:
                _r.PrintCenter($"{e.ItemName} 사용!", ConsoleColor.Cyan);
                _r.Pause(400);
                break;

            // --- 숙련도 (P2) ---
            case MasteryLevelUpEvent e:
                _r.Flash(ConsoleColor.Blue);
                _r.PrintCenter($"장인 숙련도 UP! Lv.{e.NewLevel}");
                _r.PrintCenter($"보상: {e.RewardDescription}");
                _r.WaitKey();
                break;

            // --- 에러 ---
            case CommandRejectedEvent e:
                _r.PrintCenter($"[!] {GetRejectMessage(e.Reason)}", ConsoleColor.DarkRed);
                _r.Pause(500);
                break;
        }
    }

    private static string GetRejectMessage(string reason) => reason switch
    {
        "insufficient_gold" => "골드가 부족합니다",
        "cannot_sell_wooden_sword" => "나무검은 판매할 수 없습니다",
        "level_too_low" => "+10 이상만 수집 가능합니다",
        "not_wooden_sword" => "나무검 상태에서만 지원금을 받을 수 있습니다",
        "has_enough_gold" => "강화할 골드가 충분합니다",
        _ => reason
    };
}
```

### ConsoleRenderer 주요 메서드

```csharp
public class ConsoleRenderer
{
    private int _width;  // 터미널 폭 (초기화 시 감지)

    public ConsoleRenderer()
    {
        // 터미널 폭 감지 — 실패 시 기본 40칸
        try { _width = Math.Max(Console.WindowWidth, 36); }
        catch { _width = 40; }
    }

    public void Flash(ConsoleColor color);    // 배경색 잠깐 변경 후 복원
    public void PrintCenter(string text, ConsoleColor? color = null);
    public void Pause(int ms);               // Thread.Sleep 래핑
    public void WaitKey();                    // "[아무 키] 계속" + ReadKey
    public void DrawBox(string[] lines);      // _width 기준 테두리 박스 출력
    public void DrawProgressBar(int cur, int max, int width); // ████░░░░
    public void DrawSeparator();             // ├───...───┤
}
```

### 터미널 호환성

| 환경 | 유니코드 | 색상 | 비고 |
|------|---------|------|------|
| Windows Terminal | O | O | 권장 환경 |
| PowerShell 7+ | O | O | |
| cmd.exe | △ | O | 일부 유니코드 미표시 가능 |
| macOS/Linux Terminal | O | O | |

- 박스 문자(`┌─┐│└─┘├┤`)는 모든 환경에서 동작
- 이모지(`⚔💰✦✖`)는 cmd.exe에서 깨질 수 있음 → **ASCII 폴백 옵션** 제공
- `--ascii` CLI 옵션으로 이모지 대신 ASCII 문자 사용 (`*`, `$`, `X` 등)

---

## 키 매핑

| 키 | 화면 | 동작 | 디스패치하는 커맨드 |
|----|------|------|-------------------|
| `E` | Enhance | 강화 | `EnhanceCommand` |
| `S` | Enhance | 판매 | `SellCommand` |
| `C` | Enhance | 수집 (+10 이상) | `CollectCommand` |
| `F` | Enhance | 긴급 지원금 (P1) | `EmergencyFundCommand` |
| `W` | Enhance | 공방으로 이동 | 화면 전환 |
| `1-3` | Workshop | 파편 교환 | `ExchangeCommand` |
| `B` | Workshop | 대장간으로 복귀 | 화면 전환 |
| `Q` | 전체 | 종료 (저장 후) | - |

- Unity 뷰에서 버튼 터치 → `engine.Dispatch(Command)`
- CUI에서 키 입력 → 동일한 `engine.Dispatch(Command)`
- **통신 경로가 완전히 동일**

---

## 검 ASCII 아트

단계별로 다른 아트를 표시하여 시각적 재미를 준다.

```csharp
public static class SwordAsciiArt
{
    public static string[] Get(int level) => level switch
    {
        0 => WoodenSword,      // +0 나무검: 단순한 막대기
        <= 5 => BasicSword,    // +1~5: 기본 검 형태
        <= 9 => MagicSword,    // +6~9: 빛나는 룬 장식
        <= 14 => LegendarySword, // +10~14: 화려한 전설 검
        <= 17 => MythicSword,   // +15~17: 오라 + 불꽃
        _ => CosmicSword,       // +18~20: 우주적 이펙트
    };
}
```

예시 — 나무검 (+0):
```
    |
    |
   ===
    |
```

전설 검 (+10~14):
```
   \✦/
    ✦
   /✦\
  / ✦ \
 /  ✦  \
/___✦___\
    ‖
   ═╬═
```

---

## Unity 뷰와의 대응 관계

| Unity (App/) | CUI (CuiApp/) | 역할 |
|-------------|---------------|------|
| `GameBinding.cs` | `Program.cs` | DI 설정 + 엔트리포인트 |
| `EnhanceView.cs` | `EnhanceScreen.cs` | 강화 화면 |
| `WorkshopView.cs` | `WorkshopScreen.cs` | 공방 화면 |
| `TitleView.cs` | `TitleScreen.cs` | 타이틀 화면 |
| `ScreenManager.cs` | `ScreenManager.cs` | 화면 전환 |
| `BasicEnhanceAnimation.cs` | `EventHandler.cs` | 연출 (색상 변경, 딜레이) |
| `LocalStorageRepository.cs` | `JsonFileRepository.cs` | 저장 |
| Unity `Random` | `ConsoleRandomProvider` | 난수 |
| Unity `DateTime` | `ConsoleTimeProvider` | 시간 |

---

## P1 CUI 제약사항

- `GameContext.IsAdSystemEnabled = false` — Unity와 동일
- 이 설정에 의해 `EnhanceLogic.HandleFail()`에서 `pendingAdProtection`이 항상 false
- 따라서 광고 보호권 대기 상태에 빠지는 **데드락이 구조적으로 불가능**
- 광고 관련 키(`WatchAdCommand`, `ConfirmDestroyCommand`)는 P1에서 노출하지 않는다
- 골드 고갈 방지: `EmergencyFundCommand`(나무검 + 골드 < 강화비용 시 200G 지급)
- 연출: 색상 변경 + 딜레이 + 텍스트 이펙트 (사운드/진동 없음)

---

## CLI 옵션

```
CuiApp [options]

옵션:
  --seed <int>     난수 시드 고정 (디버그/리플레이용)
  --ascii          이모지 대신 ASCII 문자 사용 (cmd.exe 호환)
  --save <path>    세이브 파일 경로 지정 (기본: ./save.json)
  --no-color       색상 출력 비활성화
```

---

## CUI 태스크 리스트

> **전제**: GameCore가 먼저 구현되어 있어야 한다 (tasks.md P0~P1).
> CUI 태스크는 GameCore P1 완료 후 착수.

### C0: 프로젝트 셋업
- [ ] C0-1. `CuiApp/` 솔루션 + `CuiApp.csproj` 생성 (GameCore 소스 Compile Include)
- [ ] C0-2. `ConsoleRandomProvider` 구현 (시드 기반, `--seed` 옵션 지원)
- [ ] C0-3. `ConsoleTimeProvider` 구현
- [ ] C0-4. `JsonFileRepository` 구현 (동기 Load/Save, `--save` 경로 지원)
- [ ] C0-5. `ConsoleRenderer` 구현 (DrawBox, PrintCenter, Flash, Pause, WaitKey, DrawProgressBar, 터미널 폭 감지, `--ascii`/`--no-color` 대응)
- [ ] C0-6. `Program.cs` — CLI 옵션 파싱 + 초기화 + 게임 루프 뼈대
- [ ] C0-7. 빌드 & 실행 확인 (`dotnet run`으로 타이틀 화면 표시까지)

### C1: 핵심 화면
- [ ] C1-1. `IScreen` 인터페이스 + `ScreenManager` 구현 (전환 메커니즘, 입력 잠금)
- [ ] C1-2. `TitleScreen` — 타이틀 표시 + `S`키로 게임 시작
- [ ] C1-3. `EnhanceScreen` — 5가지 상태별 분기 렌더링 (나무검/일반/수집가능/골드부족/아이템장착)
- [ ] C1-4. `SwordAsciiArt` — 단계별 ASCII 아트 6종 (나무검/기본/마법/전설/신화/우주)
- [ ] C1-5. `EventRenderer` — 이벤트 큐잉 + 순차 연출 (성공/실패/판매/수집/reject/긴급지원금)
- [ ] C1-6. 자동 저장 로직 — `EventRenderer.ShouldSave` 판정 + `repo.Save()` 호출
- [ ] C1-7. 긴급 지원금 — 조건부 `[F]` 표시 + `EmergencyFundCommand` 디스패치

### C2: 서브 시스템 화면 (GameCore P2 완료 후)
- [ ] C2-1. `WorkshopScreen` — 3섹션 렌더링 (숙련도 바, 파편 교환소, 컬렉션 도감)
- [ ] C2-2. 파편 교환 입력 (`1`~`3` 키 → `ExchangeCommand`)
- [ ] C2-3. 컬렉션 도감 표시 (수집 `✦`/미수집 `□`, 별 표시 `★★`, 완성도 N/11)
- [ ] C2-4. 아이템 사용 (`UseItemCommand`) — EnhanceScreen에서 `[I]` 키로 아이템 서브메뉴
- [ ] C2-5. 통계 표시 (최고 강화, 총 파괴, 총 강화 시도, 연속 기록)

### C3: 폴리싱
- [ ] C3-1. 고단계 연출 — 서스펜스 딜레이 단계별 증가 (+10: 1.2s, +15: 2s, +18: 3s)
- [ ] C3-2. 파괴 도발 메시지 랜덤 풀 (5종 이상)
- [ ] C3-3. 앱 시작 시 세이브 파일 유무 안내 ("이어하기" / "새 게임")
- [ ] C3-4. 게임 통계 요약 화면 (종료 시 세션 리포트: 강화 N회, 파괴 N회, 골드 변동)

### CT: 테스트
- [ ] CT-1. `CuiApp.Tests.csproj` 생성 (NUnit, GameCore 소스 참조)
- [ ] CT-2. 스모크 테스트 — Program 초기화 → TitleScreen 렌더 → EnhanceScreen 전환 정상 동작
- [ ] CT-3. ScreenManager 전환 테스트 — HandleInput 반환값에 따른 화면 전환, 입력 잠금 동작
- [ ] CT-4. EventRenderer 테스트 — 이벤트 큐잉/ShouldSave 판정 (렌더 출력은 검증하지 않음)
- [ ] CT-5. EnhanceScreen 상태 분기 테스트 — 나무검/일반/수집가능/골드부족 각 상태에서 올바른 키 매핑
- [ ] CT-6. JsonFileRepository 라운드트립 테스트 — Save → Load → 데이터 일치
