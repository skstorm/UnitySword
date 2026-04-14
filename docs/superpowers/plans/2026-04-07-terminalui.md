# TerminalUI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Spectre.Console Live Display 기반 Terminal UI를 새 프로젝트로 구현. 같은 GameCore 로직을 사용하여 3단 레이아웃(리소스바/게임영역/커맨드바) + 검 ASCII 아트로 검강화 게임을 표시한다.

**Architecture:** TerminalUI 프로젝트가 GameCore를 와일드카드 참조. 각 View 클래스(ResourceBar, SwordView, ShopView, CommandBar)는 GameState를 받아 Spectre IRenderable을 반환하는 순수 렌더러. GameScreen이 Live 루프를 주관하고 키 입력을 Command로 라우팅한다.

**Tech Stack:** .NET 8.0, Spectre.Console, GameCore

---

## File Structure

```
TerminalUI/
  TerminalUI.csproj     — Spectre.Console NuGet + GameCore 와일드카드 참조
  Program.cs            — 진입점, CSV 로드, GameEngine 생성, GameScreen 시작
  SwordArt.cs           — Theme별 ASCII 아트 데이터 (순수 데이터)
  EventFormatter.cs     — GameEvent → Spectre 마크업 문자열 변환
  ResourceBar.cs        — 상단 영구 리소스 바 렌더러
  SwordView.cs          — 중앙 게임 영역 렌더러 (검 아트 + 강화 정보)
  ShopView.cs           — 상점 화면 렌더러
  CommandBar.cs         — 하단 커맨드 영역 렌더러
  GameScreen.cs         — Live Display 루프, 레이아웃 조립, 키 입력 분배
  SaveManager.cs        — ConsoleApp에서 복사 (동일 로직)
```

---

### Task 1: 프로젝트 셋업 + csproj

**Files:**
- Create: `TerminalUI/TerminalUI.csproj`

- [ ] **Step 1: csproj 파일 생성**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>TerminalUI</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="..\Project\Assets\Scripts\GameCore\**\*.cs" LinkBase="GameCore" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Spectre.Console" Version="0.49.1" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="..\Project\Assets\Resources\Data\swords.csv" CopyToOutputDirectory="PreserveNewest" Link="Data\swords.csv" />
    <Content Include="..\doc\mastery_levels.csv" CopyToOutputDirectory="PreserveNewest" Link="Data\mastery_levels.csv" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: 최소 Program.cs 생성**

```csharp
using System;

namespace TerminalUI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("TerminalUI starting...");
        }
    }
}
```

- [ ] **Step 3: 빌드 확인**

Run: `dotnet build TerminalUI/TerminalUI.csproj`
Expected: Build succeeded

- [ ] **Step 4: Commit**

```bash
git add TerminalUI/TerminalUI.csproj TerminalUI/Program.cs
git commit -m "feat: create TerminalUI project with Spectre.Console"
```

---

### Task 2: SaveManager 복사

**Files:**
- Create: `TerminalUI/SaveManager.cs`

- [ ] **Step 1: ConsoleApp/SaveManager.cs를 복사하고 namespace 변경**

ConsoleApp/SaveManager.cs를 그대로 복사하되 namespace만 변경:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GameCore.Models;

namespace TerminalUI
{
    /// <summary>
    /// 파일 시스템 기반 PlayerData 저장/로드 관리자.
    /// PlayerData를 JSON DTO(SaveData)로 변환하여 save.json에 기록한다.
    /// </summary>
    public class SaveManager
    {
        private readonly string _savePath;

        /// <summary>마지막 Load에서 파일 손상이 감지되었는지 여부.</summary>
        public bool LastLoadCorrupted { get; private set; }

        public SaveManager(string savePath = null)
        {
            _savePath = savePath ?? Path.Combine(AppContext.BaseDirectory, "save.json");
        }

        /// <summary>PlayerData를 JSON 파일로 직렬화하여 저장한다.</summary>
        public void Save(PlayerData data)
        {
            var saveData = new SaveData
            {
                Gold = data.Gold,
                Fragments = data.Fragments,
                MasteryLevel = data.Mastery.Level,
                MasteryTotalAttempts = data.Mastery.TotalAttempts,
                ProtectionAmulets = data.Inventory.ProtectionAmulets,
                Collected = new Dictionary<int, int>(data.Collection.Collected),
                TotalCollectible = data.Collection.TotalCollectible,
                HighestEnhanceLevel = data.Stats.HighestEnhanceLevel,
                WeeklyHighestLevel = data.Stats.WeeklyHighestLevel,
                TotalDestroys = data.Stats.TotalDestroys,
                TotalEnhanceAttempts = data.Stats.TotalEnhanceAttempts,
                TotalSells = data.Stats.TotalSells,
                TotalGoldEarned = data.Stats.TotalGoldEarned,
                TotalGoldSpent = data.Stats.TotalGoldSpent,
                MaxConsecutiveSuccess = data.Stats.MaxConsecutiveSuccess,
                MaxConsecutiveFail = data.Stats.MaxConsecutiveFail
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(saveData, options);
            File.WriteAllText(_savePath, json);
        }

        /// <summary>
        /// JSON 파일에서 PlayerData를 역직렬화하여 로드한다.
        /// 파일이 없거나 손상된 경우 null을 반환한다.
        /// </summary>
        public PlayerData Load()
        {
            LastLoadCorrupted = false;

            if (!File.Exists(_savePath))
                return null;

            try
            {
                var json = File.ReadAllText(_savePath);
                var saveData = JsonSerializer.Deserialize<SaveData>(json);

                if (saveData == null)
                {
                    LastLoadCorrupted = true;
                    return null;
                }

                var statistics = new Statistics(
                    highestEnhanceLevel: saveData.HighestEnhanceLevel,
                    weeklyHighestLevel: saveData.WeeklyHighestLevel,
                    totalDestroys: saveData.TotalDestroys,
                    totalEnhanceAttempts: saveData.TotalEnhanceAttempts,
                    totalSells: saveData.TotalSells,
                    totalGoldEarned: saveData.TotalGoldEarned,
                    totalGoldSpent: saveData.TotalGoldSpent,
                    maxConsecutiveSuccess: saveData.MaxConsecutiveSuccess,
                    maxConsecutiveFail: saveData.MaxConsecutiveFail
                );

                var mastery = new MasteryData(
                    level: saveData.MasteryLevel,
                    totalAttempts: saveData.MasteryTotalAttempts
                );

                var inventory = new Inventory(
                    protectionAmulets: saveData.ProtectionAmulets
                );

                var collection = new CollectionData(
                    collected: saveData.Collected ?? new Dictionary<int, int>(),
                    totalCollectible: saveData.TotalCollectible
                );

                return new PlayerData(
                    gold: saveData.Gold,
                    stats: statistics,
                    fragments: saveData.Fragments,
                    mastery: mastery,
                    collection: collection,
                    inventory: inventory,
                    isFirstRun: false
                );
            }
            catch (JsonException)
            {
                LastLoadCorrupted = true;
                return null;
            }
        }

        /// <summary>JSON 직렬화용 DTO.</summary>
        private class SaveData
        {
            public int Gold { get; set; }
            public int Fragments { get; set; }
            public int MasteryLevel { get; set; }
            public int MasteryTotalAttempts { get; set; }
            public int ProtectionAmulets { get; set; }
            public Dictionary<int, int> Collected { get; set; }
            public int TotalCollectible { get; set; }
            public int HighestEnhanceLevel { get; set; }
            public int WeeklyHighestLevel { get; set; }
            public int TotalDestroys { get; set; }
            public int TotalEnhanceAttempts { get; set; }
            public int TotalSells { get; set; }
            public int TotalGoldEarned { get; set; }
            public int TotalGoldSpent { get; set; }
            public int MaxConsecutiveSuccess { get; set; }
            public int MaxConsecutiveFail { get; set; }
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build TerminalUI/TerminalUI.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add TerminalUI/SaveManager.cs
git commit -m "feat: add SaveManager to TerminalUI"
```

---

### Task 3: SwordArt — 검 ASCII 아트 데이터

**Files:**
- Create: `TerminalUI/SwordArt.cs`

- [ ] **Step 1: SwordArt 클래스 작성**

swords.csv의 검 이름을 기준으로 테마 그룹을 나누고, 각 그룹별 ASCII 아트를 정의한다.
레벨 범위: +0(나무검), +1~3(기본), +4~5(기본 상위), +6~8(판타지), +9(역사), +10~12(전설), +13~14(신화), +15~16(신화 상위), +17(서브컬쳐), +18~20(뇌절).

실제 테마 컬럼이 없으므로 검 이름(Sword.Name) 기준으로 매핑한다.

```csharp
using System.Collections.Generic;

namespace TerminalUI
{
    /// <summary>
    /// 검 레벨별 ASCII 아트 데이터.
    /// GetArt(level)로 문자열 배열을 반환한다.
    /// </summary>
    public static class SwordArt
    {
        /// <summary>레벨에 해당하는 ASCII 아트 라인 배열을 반환한다.</summary>
        public static string[] GetArt(int level)
        {
            if (level <= 0) return Wooden;
            if (level <= 3) return Iron;
            if (level <= 5) return Steel;
            if (level <= 8) return Fantasy;
            if (level <= 9) return Historic;
            if (level <= 12) return Legendary;
            if (level <= 14) return Mythic;
            if (level <= 16) return Divine;
            if (level <= 17) return Subculture;
            return Transcendent;
        }

        /// <summary>레벨에 해당하는 Spectre 마크업 색상을 반환한다.</summary>
        public static string GetColor(int level)
        {
            if (level <= 0) return "grey";
            if (level <= 3) return "silver";
            if (level <= 5) return "white";
            if (level <= 8) return "dodgerblue1";
            if (level <= 9) return "darkorange";
            if (level <= 12) return "gold1";
            if (level <= 14) return "orchid";
            if (level <= 16) return "red1";
            if (level <= 17) return "deeppink1";
            return "yellow";
        }

        private static readonly string[] Wooden = new[]
        {
            "      │      ",
            "      │      ",
            "      │      ",
            "      │      ",
            "      │      ",
            "     ─┼─     ",
        };

        private static readonly string[] Iron = new[]
        {
            "      ╱╲     ",
            "     ╱  ╲    ",
            "     ╲  ╱    ",
            "      ║║     ",
            "      ║║     ",
            "     ─╨╨─    ",
        };

        private static readonly string[] Steel = new[]
        {
            "      ╱╲     ",
            "     ╱▪▪╲    ",
            "     ╲▪▪╱    ",
            "      ║║     ",
            "     ═╩╩═    ",
            "             ",
        };

        private static readonly string[] Fantasy = new[]
        {
            "      ╱╲     ",
            "     ╱✦✦╲    ",
            "     ╲✦✦╱    ",
            "      ║║║    ",
            "     ═╬╩╬═   ",
            "             ",
        };

        private static readonly string[] Historic = new[]
        {
            "     ╱╲╱╲    ",
            "    ╱ ◇◇ ╲   ",
            "    ╲ ◇◇ ╱   ",
            "     ╲  ╱    ",
            "      ║║     ",
            "    ══╩╩══   ",
        };

        private static readonly string[] Legendary = new[]
        {
            "      ╱◆╲    ",
            "     ╱◆◆◆╲   ",
            "     ╲◆◆◆╱   ",
            "      ║██║   ",
            "     ═╩══╩═  ",
            "             ",
        };

        private static readonly string[] Mythic = new[]
        {
            "    ✧ ╱╲ ✧   ",
            "     ╱▓▓╲    ",
            "     ╲▓▓╱    ",
            "      ║▓║    ",
            "    ═╩════╩═ ",
            "             ",
        };

        private static readonly string[] Divine = new[]
        {
            "   ✧  ╱╲  ✧  ",
            "    ✧╱░░╲✧   ",
            "     ╲░░╱    ",
            "      ║░║    ",
            "   ═╩══════╩═",
            "             ",
        };

        private static readonly string[] Subculture = new[]
        {
            "  * ✧ ╱╲ ✧ * ",
            "    ╱∞∞∞∞╲   ",
            "    ╲∞∞∞∞╱   ",
            "     ║✧✧║    ",
            "  ═╩════════╩═",
            "              ",
        };

        private static readonly string[] Transcendent = new[]
        {
            " ✧ * ╱▲╲ * ✧ ",
            "   ╱██████╲   ",
            "   ╲██████╱   ",
            "    ║✧✧✧✧║   ",
            " ═╩══════════╩═",
            "               ",
        };
    }
}
```

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build TerminalUI/TerminalUI.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add TerminalUI/SwordArt.cs
git commit -m "feat: add SwordArt ASCII art data per sword tier"
```

---

### Task 4: EventFormatter — 이벤트 마크업 변환

**Files:**
- Create: `TerminalUI/EventFormatter.cs`

- [ ] **Step 1: EventFormatter 클래스 작성**

GameEvent를 받아 Spectre.Console 마크업 문자열을 반환한다. ConsoleRenderer.HandleEvent와 동일한 이벤트 매핑이지만, Console.WriteLine 대신 마크업 문자열을 반환한다.

```csharp
using GameCore.Events;
using GameCore.Models;

namespace TerminalUI
{
    /// <summary>
    /// GameEvent를 Spectre.Console 마크업 문자열로 변환한다.
    /// 각 이벤트 타입별 색상과 아이콘을 적용한다.
    /// </summary>
    public static class EventFormatter
    {
        /// <summary>
        /// GameEvent를 마크업 문자열로 변환한다. 표시할 필요 없는 이벤트는 null 반환.
        /// </summary>
        public static string Format(GameEvent evt)
        {
            return evt switch
            {
                EnhanceSuccessEvent e =>
                    $"[green]★ 강화 성공! +{e.PrevLevel} → +{e.NewLevel} {Escape(e.NewSwordName)}[/]",

                EnhanceFailEvent e when e.Destroyed =>
                    $"[red]✕ 파괴! +{e.DestroyedLevel} {Escape(e.DestroyedSwordName)}[/]",

                EnhanceFailEvent e when !e.Destroyed =>
                    $"[yellow]◆ 보호! 강화 실패했지만 파괴를 막았습니다.[/]",

                SellEvent e =>
                    $"[cyan]$ 판매! +{e.SoldLevel} {Escape(e.SoldSwordName)} → {e.GoldGained:N0}G[/]",

                DestroyConfirmedEvent =>
                    "[grey]나무검으로 돌아갑니다...[/]",

                FragmentGainEvent e =>
                    $"[cyan]◆ 파편 +{e.TotalGained} 획득 (보유: {e.NewTotal})[/]",

                ExchangeEvent e =>
                    $"[fuchsia]🛒 {FormatItemName(e.ItemType)} 구매 (파편 -{e.FragmentCost})[/]",

                UseItemEvent =>
                    "[blue]🛡 보호부적 활성화![/]",

                CollectEvent e =>
                    $"[green]📖 +{e.SwordLevel} {Escape(e.SwordName)} 도감에 등록! ({e.UniqueCount}/{e.TotalCollectible})[/]",

                CollectionCompleteEvent =>
                    "[yellow]🏆 도감 완성![/]",

                MasteryExpEvent e =>
                    $"[olive]⚡ 마스터리 경험치 +1 (Lv.{e.CurrentLevel})[/]",

                MasteryLevelUpEvent e =>
                    $"[yellow]⚡ 마스터리 Lv.{e.NewLevel} 달성! {Escape(e.RewardDescription)}[/]",

                CommandRejectedEvent e =>
                    $"[grey]{FormatRejection(e.Reason)}[/]",

                GoldChangeEvent => null,

                _ => null
            };
        }

        private static string FormatItemName(string itemType)
        {
            return itemType switch
            {
                ItemTypes.ProtectionAmulet => "보호부적",
                ItemTypes.GoldPouch => "골드주머니",
                _ => itemType
            };
        }

        private static string FormatRejection(string reason)
        {
            return reason switch
            {
                "insufficient_gold" => "골드가 부족합니다!",
                "cannot_sell_wooden_sword" => "나무검은 판매할 수 없습니다.",
                "max_level_reached" => "이미 최고 단계입니다!",
                "pending_ad_protection" => "파괴 처리를 먼저 완료해주세요.",
                "no_pending_destruction" => "파괴 대기 상태가 아닙니다.",
                "insufficient_fragments" => "파편이 부족합니다!",
                "no_amulets" => "보호부적이 없습니다.",
                "already_protected" => "이미 보호가 활성화되어 있습니다.",
                "cannot_protect_wooden_sword" => "나무검은 보호할 수 없습니다.",
                "not_collectible" => "수집할 수 없는 검입니다.",
                "invalid_item_type" => "잘못된 아이템입니다.",
                _ => reason
            };
        }

        /// <summary>Spectre 마크업 특수문자([, ]) 이스케이프</summary>
        private static string Escape(string text)
        {
            return text.Replace("[", "[[").Replace("]", "]]");
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build TerminalUI/TerminalUI.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add TerminalUI/EventFormatter.cs
git commit -m "feat: add EventFormatter for Spectre markup conversion"
```

---

### Task 5: ResourceBar — 상단 리소스 바

**Files:**
- Create: `TerminalUI/ResourceBar.cs`

- [ ] **Step 1: ResourceBar 클래스 작성**

GameState를 받아 영구 리소스(골드, 파편, 마스터리, 도감)를 한 줄 `Panel`로 반환한다.

```csharp
using GameCore.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace TerminalUI
{
    /// <summary>
    /// 상단 영구 리소스 바. 골드/파편/마스터리/도감 진행도를 한 줄로 표시한다.
    /// </summary>
    public static class ResourceBar
    {
        public static IRenderable Render(GameState state)
        {
            var p = state.PlayerData;
            var text = new Markup(
                $"  [yellow]💰 {p.Gold:N0}G[/]  " +
                $"[cyan]◆ {p.Fragments}파편[/]  " +
                $"[olive]⚡ Lv.{p.Mastery.Level}[/]  " +
                $"[green]📖 {p.Collection.UniqueCount}/{p.Collection.TotalCollectible}[/]"
            );

            return new Panel(text)
                .Header("[bold]리소스[/]")
                .BorderColor(Color.Grey)
                .Expand();
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build TerminalUI/TerminalUI.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add TerminalUI/ResourceBar.cs
git commit -m "feat: add ResourceBar renderer"
```

---

### Task 6: SwordView — 중앙 게임 영역

**Files:**
- Create: `TerminalUI/SwordView.cs`

- [ ] **Step 1: SwordView 클래스 작성**

검 ASCII 아트를 왼쪽에, 강화 정보를 오른쪽에 배치한다. 최근 이벤트 1줄을 하단에 표시한다.

```csharp
using GameCore.Data;
using GameCore.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace TerminalUI
{
    /// <summary>
    /// 중앙 게임 영역. 검 ASCII 아트 + 강화 정보 + 최근 이벤트를 표시한다.
    /// </summary>
    public class SwordView
    {
        private readonly SwordDataTable _swordTable;
        private string _lastEvent;

        public SwordView(SwordDataTable swordTable)
        {
            _swordTable = swordTable;
        }

        /// <summary>최근 이벤트 마크업을 설정한다.</summary>
        public void SetLastEvent(string eventMarkup)
        {
            _lastEvent = eventMarkup;
        }

        public IRenderable Render(GameState state)
        {
            var level = state.CurrentLevel;
            var sword = state.CurrentSword;
            var color = SwordArt.GetColor(level);
            var artLines = SwordArt.GetArt(level);

            // 검 아트를 색상 마크업으로 감싸기
            var artText = string.Join("\n",
                System.Array.ConvertAll(artLines, line =>
                    $"[{color}]{Escape(line)}[/]"));

            // 오른쪽 강화 정보
            var levelStr = level > 0 ? $"+{level} " : "";
            var info = $"[bold {color}]{levelStr}{Escape(sword.Name)}[/]\n";
            info += $"[grey]{Escape(sword.Theme)}[/]\n\n";

            if (level < _swordTable.MaxLevel)
            {
                var next = _swordTable.GetSword(level + 1);
                // +15 이상은 성공률을 ???로 숨김
                var rateStr = level + 1 <= 14 ? $"{next.SuccessRate * 100:F1}%" : "[red]???[/]";
                info += $"강화 확률: {rateStr}\n";
                info += $"강화 비용: [yellow]{next.EnhanceCost:N0}G[/]\n";
            }
            else
            {
                info += "[bold yellow]*** 최고 단계 달성! ***[/]\n";
            }

            if (level > 0)
                info += $"판매가: [cyan]{sword.SellPrice:N0}G[/]\n";

            info += $"보호: {(state.HasActiveProtection ? "[blue]✓ 활성[/]" : "[grey]✗[/]")}";

            // 2열 레이아웃: 검 아트 | 정보
            var columns = new Columns(
                new Markup(artText),
                new Markup(info)
            );

            // 최근 이벤트
            var eventLine = _lastEvent != null
                ? new Markup($"\n  {_lastEvent}")
                : new Markup("");

            var grid = new Grid();
            grid.AddColumn();
            grid.AddRow(columns);
            grid.AddRow(eventLine);

            return new Panel(grid)
                .Header($"[bold {color}]⚔ 대장간[/]")
                .BorderColor(Color.Grey)
                .Expand();
        }

        private static string Escape(string text)
        {
            return text.Replace("[", "[[").Replace("]", "]]");
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build TerminalUI/TerminalUI.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add TerminalUI/SwordView.cs
git commit -m "feat: add SwordView with ASCII art and enhance info"
```

---

### Task 7: ShopView — 상점 화면

**Files:**
- Create: `TerminalUI/ShopView.cs`

- [ ] **Step 1: ShopView 클래스 작성**

상점 진입 시 SwordView 대신 표시되는 렌더러. ExchangeCommand 상수를 참조하여 비용을 표시한다.

```csharp
using GameCore.Commands;
using GameCore.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace TerminalUI
{
    /// <summary>
    /// 상점 화면. 파편으로 교환 가능한 아이템 목록을 표시한다.
    /// 비용은 ExchangeCommand 상수를 참조하여 표시/로직 간 불일치를 방지한다.
    /// </summary>
    public static class ShopView
    {
        public static IRenderable Render(GameState state)
        {
            var grid = new Grid();
            grid.AddColumn();

            grid.AddRow(new Markup(
                $"  [blue]🛡 보호부적[/]         " +
                $"[cyan]{ExchangeCommand.ProtectionAmuletCost} 파편[/]\n" +
                $"  [grey]   강화 실패 시 파괴를 1회 방지[/]"));

            grid.AddEmptyRow();

            grid.AddRow(new Markup(
                $"  [yellow]💰 골드주머니[/]       " +
                $"[cyan]{ExchangeCommand.GoldPouchCost} 파편[/] → " +
                $"[yellow]{ExchangeCommand.GoldPouchReward}G[/]\n" +
                $"  [grey]   즉시 골드 획득[/]"));

            return new Panel(grid)
                .Header("[bold fuchsia]🏪 상점[/]")
                .BorderColor(Color.Fuchsia1)
                .Expand();
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build TerminalUI/TerminalUI.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add TerminalUI/ShopView.cs
git commit -m "feat: add ShopView renderer"
```

---

### Task 8: CommandBar — 하단 커맨드 영역

**Files:**
- Create: `TerminalUI/CommandBar.cs`

- [ ] **Step 1: CommandBar 클래스 작성**

현재 GameState와 화면 모드(메인/상점/파괴대기)에 따라 사용 가능한 커맨드를 표시한다.

```csharp
using GameCore.Data;
using GameCore.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace TerminalUI
{
    /// <summary>
    /// 하단 커맨드 영역. 현재 상태와 화면 모드에 따라 사용 가능한 키 안내를 표시한다.
    /// </summary>
    public static class CommandBar
    {
        /// <summary>메인 게임 화면의 커맨드를 렌더링한다.</summary>
        public static IRenderable RenderMain(GameState state, SwordDataTable swordTable)
        {
            if (state.PendingAdProtection)
            {
                return new Panel(new Markup(
                    "  [bold red][[3]][/] 파괴 확정    [bold][[Q]][/] 종료"))
                    .BorderColor(Color.Grey)
                    .Expand();
            }

            var parts = "";

            if (state.CurrentLevel < swordTable.MaxLevel)
                parts += "[bold green][[1]][/] 강화  ";

            if (state.CurrentLevel > 0)
                parts += "[bold cyan][[2]][/] 판매  ";

            if (state.CurrentSword.Collectible)
                parts += "[bold green][[3]][/] 수집  ";

            if (state.PlayerData.Inventory.ProtectionAmulets > 0)
                parts += "[bold blue][[4]][/] 아이템  ";

            parts += "[bold fuchsia][[5]][/] 상점  ";
            parts += "[bold][[Q]][/] 종료";

            return new Panel(new Markup($"  {parts}"))
                .BorderColor(Color.Grey)
                .Expand();
        }

        /// <summary>상점 화면의 커맨드를 렌더링한다.</summary>
        public static IRenderable RenderShop()
        {
            return new Panel(new Markup(
                "  [bold blue][[1]][/] 보호부적  " +
                "[bold yellow][[2]][/] 골드주머니  " +
                "[bold][[0]][/] 돌아가기"))
                .BorderColor(Color.Fuchsia1)
                .Expand();
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build TerminalUI/TerminalUI.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add TerminalUI/CommandBar.cs
git commit -m "feat: add CommandBar renderer"
```

---

### Task 9: GameScreen — Live Display 메인 루프

**Files:**
- Create: `TerminalUI/GameScreen.cs`

- [ ] **Step 1: GameScreen 클래스 작성**

Live Display로 3단 레이아웃을 구성하고, 키 입력을 받아 Command를 디스패치한다.

```csharp
using System;
using System.Threading;
using GameCore.Commands;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;
using Spectre.Console;

namespace TerminalUI
{
    /// <summary>
    /// Live Display 메인 루프. 3단 레이아웃(리소스/게임/커맨드)을 조립하고,
    /// 키 입력을 받아 GameEngine에 Command를 디스패치한다.
    /// </summary>
    public class GameScreen
    {
        private readonly GameEngine _engine;
        private readonly SwordDataTable _swordTable;
        private readonly SaveManager _saveManager;
        private readonly SwordView _swordView;
        private bool _inShop;
        private bool _running = true;

        public GameScreen(GameEngine engine, SwordDataTable swordTable, SaveManager saveManager)
        {
            _engine = engine;
            _swordTable = swordTable;
            _saveManager = saveManager;
            _swordView = new SwordView(swordTable);

            // 이벤트 구독: 최근 이벤트를 SwordView에 전달
            _engine.OnEvent += HandleEvent;
        }

        private void HandleEvent(GameEvent evt)
        {
            var formatted = EventFormatter.Format(evt);
            if (formatted != null)
                _swordView.SetLastEvent(formatted);
        }

        /// <summary>Command를 디스패치하고 변경된 PlayerData를 즉시 저장한다.</summary>
        private void DispatchAndSave(Command cmd)
        {
            _engine.Dispatch(cmd);
            _saveManager.Save(_engine.State.PlayerData);
        }

        /// <summary>Live Display 게임 루프를 시작한다.</summary>
        public void Run()
        {
            AnsiConsole.Clear();

            while (_running)
            {
                // 화면 렌더
                AnsiConsole.Clear();
                RenderScreen();

                // 키 입력 대기
                var key = Console.ReadKey(true).Key;

                if (_inShop)
                    HandleShopInput(key);
                else
                    HandleMainInput(key);
            }

            AnsiConsole.MarkupLine("\n[grey]게임을 종료합니다.[/]");
        }

        private void RenderScreen()
        {
            var state = _engine.State;

            // 상단: 리소스 바
            AnsiConsole.Write(ResourceBar.Render(state));

            // 중앙: 게임 영역 또는 상점
            if (_inShop)
                AnsiConsole.Write(ShopView.Render(state));
            else
                AnsiConsole.Write(_swordView.Render(state));

            // 하단: 커맨드 바
            if (_inShop)
                AnsiConsole.Write(CommandBar.RenderShop());
            else
                AnsiConsole.Write(CommandBar.RenderMain(state, _swordTable));
        }

        private void HandleMainInput(ConsoleKey key)
        {
            var state = _engine.State;

            switch (key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    if (!state.PendingAdProtection)
                        DispatchAndSave(new EnhanceCommand());
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    if (!state.PendingAdProtection)
                        DispatchAndSave(new SellCommand());
                    break;

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    if (state.PendingAdProtection)
                        DispatchAndSave(new ConfirmDestroyCommand());
                    else
                        DispatchAndSave(new CollectCommand());
                    break;

                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    if (!state.PendingAdProtection)
                        DispatchAndSave(new UseItemCommand());
                    break;

                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    if (!state.PendingAdProtection)
                        _inShop = true;
                    break;

                case ConsoleKey.Q:
                    _running = false;
                    break;
            }
        }

        private void HandleShopInput(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    DispatchAndSave(new ExchangeCommand(ItemTypes.ProtectionAmulet));
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    DispatchAndSave(new ExchangeCommand(ItemTypes.GoldPouch));
                    break;

                case ConsoleKey.D0:
                case ConsoleKey.NumPad0:
                    _inShop = false;
                    _swordView.SetLastEvent(null);
                    break;
            }
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build TerminalUI/TerminalUI.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add TerminalUI/GameScreen.cs
git commit -m "feat: add GameScreen with Live Display loop"
```

---

### Task 10: Program.cs — 진입점 완성

**Files:**
- Modify: `TerminalUI/Program.cs`

- [ ] **Step 1: Program.cs 재작성**

ConsoleApp/Program.cs의 CSV 로드 패턴을 재사용하되, ConsoleRenderer/MenuHandler 대신 GameScreen을 사용한다.

```csharp
using System;
using System.IO;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Models;
using GameCore.Util;
using Spectre.Console;

namespace TerminalUI
{
    /// <summary>
    /// TerminalUI 진입점. CSV 데이터 로드 → 세이브 로드 → GameEngine 생성 → GameScreen 시작.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // 1. CSV 데이터 로드
            var csvContent = LoadCsv("swords.csv",
                Path.Combine("Project", "Assets", "Resources", "Data", "swords.csv"));
            var swords = new SwordDataLoader().Parse(csvContent);
            var swordTable = new SwordDataTable(swords);

            // 2. 마스터리 데이터 로드
            MasteryDataTable masteryTable = null;
            var masteryCsv = LoadCsvOptional("mastery_levels.csv",
                Path.Combine("doc", "mastery_levels.csv"));
            if (masteryCsv != null)
            {
                var masteryLevels = new MasteryDataLoader().Parse(masteryCsv);
                masteryTable = new MasteryDataTable(masteryLevels);
            }

            // 3. 세이브 로드
            var saveManager = new SaveManager();
            var playerData = saveManager.Load() ?? new PlayerData();
            if (saveManager.LastLoadCorrupted)
                AnsiConsole.MarkupLine("[red]  [!] 저장 파일이 손상되어 새로 시작합니다.[/]");

            // 4. GameEngine 생성
            var random = new SeededRandomProvider(Environment.TickCount);
            var time = new FakeTimeProvider();
            var context = new GameContext(random, time, swordTable, masteryTable);
            var engine = GameEngine.Create(playerData, context);

            // 5. GameScreen 시작
            var screen = new GameScreen(engine, swordTable, saveManager);
            screen.Run();
        }

        /// <summary>필수 CSV 로드. 여러 경로에서 순서대로 탐색한다.</summary>
        static string LoadCsv(string fileName, string fallbackRelative)
        {
            var paths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "Data", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", fallbackRelative)
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                    return File.ReadAllText(path);
            }

            throw new FileNotFoundException(
                $"{fileName} not found. Searched: {string.Join(", ", paths)}");
        }

        /// <summary>선택적 CSV 로드. 파일이 없으면 null 반환.</summary>
        static string LoadCsvOptional(string fileName, string fallbackRelative)
        {
            try { return LoadCsv(fileName, fallbackRelative); }
            catch (FileNotFoundException) { return null; }
        }
    }
}
```

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build TerminalUI/TerminalUI.csproj`
Expected: Build succeeded

- [ ] **Step 3: 실행 확인**

Run: `dotnet run --project TerminalUI/TerminalUI.csproj`
Expected: 3단 레이아웃 표시, 키 입력 반응, 강화/판매/상점 동작

- [ ] **Step 4: run-tui.bat 생성**

```batch
@echo off
cd /d "%~dp0"
dotnet run --project TerminalUI\TerminalUI.csproj
pause
```

- [ ] **Step 5: Commit**

```bash
git add TerminalUI/Program.cs run-tui.bat
git commit -m "feat: complete TerminalUI with Program entry point"
```

---

### Task 11: 통합 확인

- [ ] **Step 1: 클린 빌드**

Run: `dotnet build TerminalUI/TerminalUI.csproj --no-incremental`
Expected: Build succeeded

- [ ] **Step 2: GameCore 테스트 통과 확인**

Run: `dotnet test GameCore.Tests/GameCore.Tests.csproj`
Expected: 152 tests passed

- [ ] **Step 3: 수동 플레이 테스트**

전체 시나리오:
1. 강화 반복 → 파괴 발생 → 파편 획득 확인, 검 아트 변경 확인
2. 상점([5]) → 보호부적 구매([1]) → 돌아가기([0]) → 아이템 사용([4]) → 보호 표시 확인
3. 상점 → 골드주머니 구매([2]) → 골드 증가 확인
4. +10 이상 검 → 수집([3]) → 도감 진행도 변경 확인
5. [Q] 종료 → 재실행 → 데이터 유지 확인

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "feat: TerminalUI implementation complete"
```
