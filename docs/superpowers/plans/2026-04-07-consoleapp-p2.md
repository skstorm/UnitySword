# ConsoleApp P2 기능 반영 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** ConsoleApp에 P2 기능(마스터리, 파편, 교환, 아이템 사용, 수집) 전부 반영 + JSON 저장/로드

**Architecture:** Program.cs를 ConsoleRenderer, MenuHandler, SaveManager로 분리. 기존 P1 동작 유지하면서 P2 커맨드/이벤트 추가.

**Tech Stack:** .NET 8.0, System.Text.Json, GameCore

---

## File Structure

```
ConsoleApp/
  Program.cs              — 진입점, CSV/mastery 로드, 게임 루프 (기존 리팩토링)
  ConsoleRenderer.cs      — 상태 표시 + 이벤트 색상 출력 (NEW)
  MenuHandler.cs          — 메인/상점/파괴대기 메뉴 처리 (NEW)
  SaveManager.cs          — PlayerData JSON 저장/로드 (NEW)
  ConsoleApp.csproj       — mastery_levels.csv 추가 (MODIFY)
```

---

### Task 1: csproj에 mastery_levels.csv 추가

**Files:**
- Modify: `ConsoleApp/ConsoleApp.csproj`

- [ ] **Step 1: mastery_levels.csv Content 항목 추가**

```xml
<Content Include="..\Project\Assets\Resources\Data\mastery_levels.csv" 
         CopyToOutputDirectory="PreserveNewest" Link="Data\mastery_levels.csv" />
```

기존 swords.csv Content 항목 아래에 추가.

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build ConsoleApp/ConsoleApp.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add ConsoleApp/ConsoleApp.csproj
git commit -m "chore: add mastery_levels.csv to ConsoleApp output"
```

---

### Task 2: SaveManager 생성

**Files:**
- Create: `ConsoleApp/SaveManager.cs`

- [ ] **Step 1: SaveManager 클래스 작성**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GameCore.Models;

namespace ConsoleApp
{
    public class SaveManager
    {
        private readonly string _savePath;

        public SaveManager(string savePath = null)
        {
            _savePath = savePath ?? Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "save.json");
        }

        public void Save(PlayerData data)
        {
            var dto = new SaveData
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

            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(_savePath, json);
        }

        public PlayerData Load()
        {
            if (!File.Exists(_savePath)) return null;

            try
            {
                var json = File.ReadAllText(_savePath);
                var dto = JsonSerializer.Deserialize<SaveData>(json);

                return new PlayerData(
                    gold: dto.Gold,
                    fragments: dto.Fragments,
                    mastery: new MasteryData(dto.MasteryLevel, dto.MasteryTotalAttempts),
                    inventory: new Inventory(dto.ProtectionAmulets),
                    collection: new CollectionData(
                        dto.Collected ?? new Dictionary<int, int>(),
                        dto.TotalCollectible),
                    stats: new Statistics(
                        highestEnhanceLevel: dto.HighestEnhanceLevel,
                        weeklyHighestLevel: dto.WeeklyHighestLevel,
                        totalDestroys: dto.TotalDestroys,
                        totalEnhanceAttempts: dto.TotalEnhanceAttempts,
                        totalSells: dto.TotalSells,
                        totalGoldEarned: dto.TotalGoldEarned,
                        totalGoldSpent: dto.TotalGoldSpent,
                        maxConsecutiveSuccess: dto.MaxConsecutiveSuccess,
                        maxConsecutiveFail: dto.MaxConsecutiveFail),
                    isFirstRun: false);
            }
            catch (Exception)
            {
                Console.WriteLine("  [!] 저장 파일이 손상되어 새로 시작합니다.");
                return null;
            }
        }

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

Run: `dotnet build ConsoleApp/ConsoleApp.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add ConsoleApp/SaveManager.cs
git commit -m "feat: add SaveManager for JSON save/load"
```

---

### Task 3: ConsoleRenderer 생성

**Files:**
- Create: `ConsoleApp/ConsoleRenderer.cs`

- [ ] **Step 1: ConsoleRenderer 작성**

PrintStatus — 상태 표시 (검, 골드, 파편, 마스터리, 인벤토리, 수집 진행도, 통계)
HandleEvent — P1+P2 이벤트 색상 출력:
- EnhanceSuccessEvent (Green)
- EnhanceFailEvent (Red)
- SellEvent (Cyan)
- DestroyConfirmedEvent (default)
- FragmentGainEvent (Cyan)
- ExchangeEvent (Magenta)
- UseItemEvent (Blue)
- CollectEvent (Green)
- CollectionCompleteEvent (Yellow)
- MasteryExpEvent (DarkYellow)
- MasteryLevelUpEvent (Yellow)
- CommandRejectedEvent (DarkGray) — 한글 사유 매핑 (기존 + insufficient_fragments, no_amulets, already_protected, cannot_protect_wooden_sword, not_collectible, invalid_item_type)
- GoldChangeEvent — skip (상태 표시에서 처리)

생성자: `ConsoleRenderer(SwordDataTable swordTable)` — 최대 레벨, 다음 강화 비용 등 표시용

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build ConsoleApp/ConsoleApp.csproj`

- [ ] **Step 3: Commit**

```bash
git add ConsoleApp/ConsoleRenderer.cs
git commit -m "feat: add ConsoleRenderer with P2 event display"
```

---

### Task 4: MenuHandler 생성

**Files:**
- Create: `ConsoleApp/MenuHandler.cs`

- [ ] **Step 1: MenuHandler 작성**

`MenuHandler(GameEngine engine, SwordDataTable swordTable, SaveManager saveManager)`

PrintMenu(GameState) — 상태에 따라 메뉴 출력:
- 파괴대기: `[3] 파괴 확정` 만 표시
- 일반: `[1] 강화`, `[2] 판매`, `[3] 수집`(조건부), `[4] 아이템사용`(조건부), `[5] 상점`, `[Q] 종료`

HandleInput(ConsoleKey) returns bool (false=종료):
- D1: `engine.Dispatch(new EnhanceCommand())`
- D2: `engine.Dispatch(new SellCommand())`
- D3: 파괴대기면 `ConfirmDestroyCommand`, 아니면 `CollectCommand`
- D4: `engine.Dispatch(new UseItemCommand())`
- D5: `EnterShop()` — 상점 서브루프
- Q: return false

EnterShop():
- 상점 헤더 + 파편 표시
- `[1] 보호부적 (30파편)`, `[2] 골드주머니 (10파편→500G)`, `[0] 돌아가기`
- 1: `engine.Dispatch(new ExchangeCommand(ItemTypes.ProtectionAmulet))`
- 2: `engine.Dispatch(new ExchangeCommand(ItemTypes.GoldPouch))`

Command 실행 후 `saveManager.Save(engine.State.PlayerData)` 호출.

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build ConsoleApp/ConsoleApp.csproj`

- [ ] **Step 3: Commit**

```bash
git add ConsoleApp/MenuHandler.cs
git commit -m "feat: add MenuHandler with shop submenu and P2 commands"
```

---

### Task 5: Program.cs 리팩토링

**Files:**
- Modify: `ConsoleApp/Program.cs`

- [ ] **Step 1: Program.cs 재작성**

Main 흐름:
1. `LoadCsv()` 유지 + `LoadMasteryCsv()` 추가 (같은 패턴)
2. autotest 분기 유지 (기존 그대로)
3. MasteryDataTable 생성, GameContext에 전달
4. SaveManager로 PlayerData 로드 시도 (없으면 new PlayerData())
5. GameEngine.Create(playerData, context)
6. ConsoleRenderer, MenuHandler 생성
7. engine.OnEvent += renderer.HandleEvent
8. 게임 루프: renderer.PrintStatus → menuHandler.PrintMenu → menuHandler.HandleInput

기존 PrintStatus, PrintMenu, HandleEvent 메서드 전부 제거 (ConsoleRenderer/MenuHandler로 이동됨).
RunAutoTest, LoadCsv는 Program.cs에 유지.

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build ConsoleApp/ConsoleApp.csproj`

- [ ] **Step 3: 수동 실행 테스트**

Run: `dotnet run --project ConsoleApp/ConsoleApp.csproj`
- 메인 메뉴 표시 확인
- 강화/판매 동작 확인
- 상점 진입/교환 확인
- 종료 후 save.json 생성 확인
- 재실행 시 저장 데이터 로드 확인

- [ ] **Step 4: autotest 모드 확인**

Run: `dotnet run --project ConsoleApp/ConsoleApp.csproj -- autotest`
Expected: 기존과 동일하게 동작

- [ ] **Step 5: Commit**

```bash
git add ConsoleApp/Program.cs
git commit -m "refactor: split Program.cs into Renderer/MenuHandler/SaveManager"
```

---

### Task 6: 통합 확인 및 최종 커밋

- [ ] **Step 1: 클린 빌드**

Run: `dotnet build ConsoleApp/ConsoleApp.csproj --no-incremental`

- [ ] **Step 2: GameCore 테스트 통과 확인**

Run: `dotnet test GameCore.Tests/GameCore.Tests.csproj`
Expected: 152 tests passed

- [ ] **Step 3: 수동 플레이 테스트**

전체 P2 시나리오:
1. 강화 반복 → 파괴 발생 → 파편 획득 확인
2. 상점 → 보호부적 구매 → 아이템 사용 → 강화 실패 시 보호 확인
3. 상점 → 골드주머니 구매 → 골드 증가 확인
4. +10 이상 검 → 수집 → 도감 등록 확인
5. 종료 → 재실행 → 데이터 유지 확인

- [ ] **Step 4: 최종 커밋**

```bash
git add -A
git commit -m "feat: ConsoleApp P2 integration complete"
```
