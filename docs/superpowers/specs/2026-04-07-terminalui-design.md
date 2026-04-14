# TerminalUI Design Spec

> Spectre.Console 기반 Terminal UI. 같은 GameCore 로직을 사용하여 Live Display로 검강화 게임을 구현한다.

## 프로젝트 구조

```
TerminalUI/
  TerminalUI.csproj     — Spectre.Console NuGet + GameCore 와일드카드 참조
  Program.cs            — 진입점, CSV 로드, GameEngine 생성, 화면 시작
  GameScreen.cs         — Live Display 루프, 레이아웃 조립, 키 입력 분배
  ResourceBar.cs        — 상단 영구 리소스 바 (골드/파편/마스터리/도감)
  SwordView.cs          — 중앙 게임 영역 (검 아트 + 강화 정보 + 최근 이벤트)
  ShopView.cs           — 상점 화면 (상점 진입 시 SwordView 대체)
  CommandBar.cs         — 하단 커맨드 영역 (상태별 메뉴 표시)
  SwordArt.cs           — Theme별 ASCII 아트 데이터
  EventFormatter.cs     — GameEvent → Spectre 마크업 문자열 변환
  SaveManager.cs        — ConsoleApp에서 복사 (동일 로직)
```

## 의존성

- **NuGet:** `Spectre.Console` (최신 안정 버전)
- **GameCore:** ConsoleApp과 동일한 와일드카드 `Compile Include` 방식
- **CSV:** swords.csv, mastery_levels.csv Content 복사
- **의존성 방향:** TerminalUI → GameCore (단방향)

## 3단 레이아웃

```
┌─── 검강화 대장간 ─────────────────────────────┐
│                                                │
│  💰 12,500G  ◆ 45파편  ⚡ Lv.3  📖 3/11      │ ← 영구 리소스 바
│                                                │
├────────────────────────────────────────────────┤
│                                                │
│              ╱╲                                │
│             ╱  ╲                               │
│            ╱ ◆◆ ╲        +7 흑요석검           │
│            ╲ ◆◆ ╱                              │
│             ║  ║         강화 확률: 45.0%       │ ← 게임 영역
│             ║  ║         강화 비용: 800G        │
│            ═╩══╩═        판매가: 1,200G         │
│                          보호: ✗               │
│                                                │
│  ★ 강화 성공! +6 → +7 흑요석검                 │ ← 최근 결과
│                                                │
├────────────────────────────────────────────────┤
│                                                │
│  [1]강화  [2]판매  [3]수집  [5]상점  [Q]종료   │ ← 커맨드 영역
│                                                │
└────────────────────────────────────────────────┘
```

### 영역 구분

| 영역 | 내용 | 성격 |
|------|------|------|
| **상단: 영구 리소스** | 골드, 파편, 마스터리, 도감 진행도 | 게임 세션 걸쳐 누적되는 자산 |
| **중앙: 게임 플레이** | 검 아트 + 이름/레벨, 강화 확률/비용, 판매가, 보호 상태, 최근 이벤트 1줄 | 현재 검에 대한 실시간 정보 |
| **하단: 커맨드** | 사용 가능한 액션 키 | 입력 영역 |

## 상점 화면

상점 진입 시 중앙 게임 영역만 상점 UI로 교체. 상단 리소스 바 유지.

```
├─── 상점 ───────────────────────────────────────┤
│                                                │
│  🛡 보호부적         30 파편                    │
│     강화 실패 시 파괴를 1회 방지               │
│                                                │
│  💰 골드주머니       10 파편 → 500G             │
│     즉시 골드 획득                              │
│                                                │
├────────────────────────────────────────────────┤
│  [1]보호부적  [2]골드주머니  [0]돌아가기       │
```

비용은 `ExchangeCommand.ProtectionAmuletCost`, `GoldPouchCost`, `GoldPouchReward` 상수 참조.

## 검 ASCII 아트

Theme별 다른 검 그림. `SwordArt.cs`에 상수로 정의.

```
나무검 (+0)        철검 (+1~3)       강철검 (+4~6)      미스릴검 (+7~9)

   │                 ╱╲               ╱╲                 ╱╲
   │                ╱  ╲             ╱▪▪╲              ╱✦✦╲
   │                ╲  ╱             ╲▪▪╱              ╲✦✦╱
   │                 ║║               ║║                ║║║
   │                 ║║              ═╩╩═              ═╬╩╬═
  ─┼─              ─╨╨─

흑요석검 (+10~12)  용검 (+13~15)     전설검 (+16~18)    신검 (+19~20)

  ╱◆╲              ╱🔥╲              ╱⚡╲              ✧╱∞╲✧
 ╱◆◆◆╲            ╱🔥🔥╲            ╱⚡⚡╲            ✧╱∞∞∞╲✧
 ╲◆◆◆╱            ╲🔥🔥╱            ╲⚡⚡╱            ✧╲∞∞∞╱✧
  ║██║              ║▓▓║              ║░║              ✧║✧✧║✧
 ═╩══╩═            ═╩══╩═            ═╩══╩═            ═╩════╩═
```

강화 성공 시 녹색, 파괴 시 빨간색 Spectre 마크업 이펙트.

## 클래스 책임

| 클래스 | 역할 | 의존성 |
|--------|------|--------|
| `Program` | CSV 로드, GameEngine 생성, GameScreen 시작 | GameEngine, SaveManager |
| `GameScreen` | Live 루프 주관, 3단 레이아웃 조립, 키→Command 라우팅, 저장 | GameEngine, 각 View, SaveManager |
| `ResourceBar` | GameState → 리소스 한 줄 IRenderable | 없음 |
| `SwordView` | GameState → 검 아트 + 정보 IRenderable | SwordArt, SwordDataTable |
| `ShopView` | 상점 아이템 목록 IRenderable | ExchangeCommand 상수 |
| `CommandBar` | GameState → 사용 가능 메뉴 IRenderable | 없음 |
| `EventFormatter` | GameEvent → Spectre 마크업 문자열 | 없음 |
| `SwordArt` | Theme → ASCII 문자열 배열 | 없음 (순수 데이터) |

**핵심 원칙:** 각 View는 GameState를 받아 IRenderable을 반환하는 순수 렌더러. 상태 변경 로직 없음.

## 기능 범위

ConsoleApp과 동일:
- 강화, 판매, 수집, 아이템사용, 상점(보호부적/골드주머니), 저장/로드
- 파괴대기 상태 처리
- autotest 모드는 미포함 (ConsoleApp 전용)

## Live Display 동작

1. `AnsiConsole.Live(layout)` 로 전체 화면 점유
2. 키 입력 대기
3. 키 → Command 디스패치 (GameScreen에서 라우팅)
4. OnEvent로 최근 이벤트 캡처
5. 상태/이벤트 갱신 → layout 리렌더
6. 매 커맨드 후 SaveManager.Save() 호출
