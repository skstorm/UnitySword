# CUI 엔진 구현계획서

## 개요

GameCore(순수 C#)를 그대로 참조하는 콘솔 애플리케이션.
Unity 없이 터미널에서 검강화 게임을 플레이한다.

```
UnitySword/
├── Assets/Scripts/GameCore/   ← 공유 (수정 없음)
└── CuiApp/                    ← 새로 생성
    ├── CuiApp.csproj
    ├── Program.cs
    ├── Providers/
    │   ├── ConsoleRandomProvider.cs
    │   └── ConsoleTimeProvider.cs
    ├── Repositories/
    │   └── JsonFileRepository.cs
    ├── Screens/
    │   ├── IScreen.cs
    │   ├── TitleScreen.cs
    │   ├── EnhanceScreen.cs
    │   └── WorkshopScreen.cs
    └── Rendering/
        ├── ConsoleRenderer.cs
        └── SwordAsciiArt.cs

---

## 핵심 원칙

1. **GameCore 무수정** — CuiApp은 GameCore.dll을 참조만 한다. Logic/Command/Event/Engine 전부 그대로 사용
2. **Unity 뷰와 동일한 통신 경로** — `engine.Dispatch(Command)` → Event 수신 → 화면 갱신
3. **Provider 교체만으로 동작** — RandomProvider, TimeProvider, Repository를 콘솔용으로 구현
4. **단일 스레드 동기 루프** — 콘솔이므로 async 불필요. 입력 대기 → 커맨드 실행 → 화면 갱신

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
    <Compile Include="..\Assets\Scripts\GameCore\**\*.cs" LinkBase="GameCore" />
  </ItemGroup>
  <ItemGroup>
    <Content Include="..\doc\swords.csv" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="..\doc\mastery_levels.csv" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
```

> GameCore는 Unity 의존성 0이므로 소스 파일을 그대로 컴파일할 수 있다.
> Assembly Definition 대신 csproj의 Compile Include로 소스 공유.

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
    void HandleInput(string input, GameEngine engine);
}
```

Unity의 View가 MonoBehaviour + 이벤트 구독이라면,
CUI의 Screen은 **Render(그리기) + HandleInput(입력 처리)** 2메서드 구조.

### 화면 전환

```
TitleScreen → EnhanceScreen ↔ WorkshopScreen
                             ↔ AchievementScreen (P3)
```

```csharp
public class ScreenManager
{
    private readonly Dictionary<string, IScreen> _screens;
    private IScreen _current;

    public void SwitchTo(string name) => _current = _screens[name];
    public void Render(GameState state, GameContext ctx) => _current.Render(state, ctx);
    public void HandleInput(string input, GameEngine engine) => _current.HandleInput(input, engine);
}
```

---

## 메인 게임 루프

```csharp
// Program.cs
var random = new ConsoleRandomProvider();
var time = new ConsoleTimeProvider();
var repo = new JsonFileRepository();
var swordTable = SwordDataLoader.Load("swords.csv");
var masteryTable = MasteryDataLoader.Load("mastery_levels.csv");
var context = new GameContext(random, time, swordTable, masteryTable, isAdSystemEnabled: false);
var playerData = await repo.LoadAsync();
var initialState = GameSessionLogic.CreateInitialState(playerData, swordTable);
var engine = new GameEngine(initialState, context);

engine.OnEvent += evt => EventHandler.Handle(evt, renderer);

var screens = new ScreenManager();
screens.SwitchTo("title");

while (true)
{
    Console.Clear();
    screens.Render(engine.State, context);
    var input = Console.ReadLine();
    if (input == "q") break;
    screens.HandleInput(input, engine);
}
await repo.SaveAsync(engine.State.PlayerData);
```

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

### EnhanceScreen (메인)

```
┌─────────────────────────────────┐
│  💰 1,250 G          Lv.3 장인  │
├─────────────────────────────────┤
│                                 │
│         ✦ 룬소드 +8 ✦           │
│            /|\                  │
│           / | \                 │
│          /  |  \                │
│         /   |   \               │
│        /____|____\              │
│            |||                  │
│            |||                  │
│                                 │
├─────────────────────────────────┤
│  [E] 강화 (53% / 120G)         │
│  [S] 판매 (295G)               │
│  [W] 공방    [Q] 종료           │
└─────────────────────────────────┘
```

+10 이상:
```
│  [E] 강화 (??? / 270G)         │
│  [S] 판매 (850G)               │
│  [C] 수집  [W] 공방  [Q] 종료   │
```

파괴 시:
```
│  ✖ 룬소드 +8 파괴!              │
│  "295G짜리 검이 가루가 됐다..."  │
│                                 │
│  [아무 키] 계속                  │
```

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

## CUI 이벤트 핸들링

Unity 뷰가 이벤트를 받아 애니메이션을 재생하듯, CUI는 이벤트를 받아 텍스트 연출을 한다.

```csharp
public static class EventHandler
{
    public static void Handle(GameEvent evt, ConsoleRenderer r)
    {
        switch (evt)
        {
            case EnhanceSuccessEvent e:
                r.Flash(ConsoleColor.Green);
                r.PrintCenter($"★ 강화 성공! +{e.NewLevel} {e.SwordName} ★");
                r.Pause(800);
                break;

            case EnhanceFailEvent e when e.Destroyed:
                r.Flash(ConsoleColor.Red);
                r.PrintCenter($"✖ {e.SwordName} +{e.Level} 파괴!");
                r.PrintCenter($"\"{e.SellPrice}G짜리 검이 가루가 됐다...\"");
                r.WaitKey();
                break;

            case SellEvent e:
                r.Flash(ConsoleColor.Yellow);
                r.PrintCenter($"💰 {e.SwordName} 판매! +{e.Gold}G");
                r.Pause(600);
                break;

            case CommandRejectedEvent e:
                r.PrintCenter($"[!] {e.Reason}", ConsoleColor.DarkRed);
                r.Pause(500);
                break;
        }
    }
}
```

### ConsoleRenderer 주요 메서드

```csharp
public class ConsoleRenderer
{
    public void Flash(ConsoleColor color);    // 배경색 잠깐 변경
    public void PrintCenter(string text);     // 가운데 정렬 출력
    public void Pause(int ms);               // Thread.Sleep 래핑
    public void WaitKey();                    // "아무 키나 누르세요"
    public void DrawBox(string[] lines);      // 테두리 박스 출력
    public void DrawProgressBar(int cur, int max, int width); // ████░░░░
}
```

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
- 광고 시스템 없음 → `EmergencyFundCommand`로 골드 고갈 방지
- 연출은 색상 변경 + 딜레이 + 텍스트 이펙트로 대체
- 사운드/진동 없음

---

## CUI 태스크 리스트

### C0: 프로젝트 셋업
- [ ] C0-1. `CuiApp/` 디렉토리 + `CuiApp.csproj` 생성 (GameCore 소스 참조)
- [ ] C0-2. `ConsoleRandomProvider`, `ConsoleTimeProvider` 구현
- [ ] C0-3. `JsonFileRepository` 구현
- [ ] C0-4. `ConsoleRenderer` 기본 메서드 구현 (DrawBox, PrintCenter, Flash, Pause)
- [ ] C0-5. `Program.cs` 엔트리포인트 + 게임 루프 뼈대

### C1: 핵심 화면
- [ ] C1-1. `IScreen` 인터페이스 + `ScreenManager` 구현
- [ ] C1-2. `TitleScreen` 구현
- [ ] C1-3. `EnhanceScreen` 구현 (검 표시, 강화/판매 입력, 확률/비용 표시)
- [ ] C1-4. `SwordAsciiArt` — 단계별 ASCII 아트 6종
- [ ] C1-5. `EventHandler` — 성공/실패/판매/reject 텍스트 연출
- [ ] C1-6. 긴급 지원금 표시 + 입력 처리 (나무검 + 골드 부족 시 [F] 표시)

### C2: 서브 시스템 화면
- [ ] C2-1. `WorkshopScreen` 구현 (숙련도, 파편, 컬렉션)
- [ ] C2-2. 파편 교환 입력 처리
- [ ] C2-3. 컬렉션 도감 표시 (수집/미수집, 별 표시)
- [ ] C2-4. 통계 표시 (최고 강화, 총 파괴 등)

### C3: 폴리싱
- [ ] C3-1. 고단계 연출 강화 (색상 그라데이션, 서스펜스 딜레이 증가)
- [ ] C3-2. 파괴 도발 메시지 랜덤 풀
- [ ] C3-3. 세이브/로드 안내 메시지
- [ ] C3-4. `--seed` CLI 옵션 (디버그/리플레이용)
