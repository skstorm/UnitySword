using System;
using System.IO;
using GameCore.Commands;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;
using GameCore.Util;

namespace ConsoleApp
{
    class Program
    {
        static GameEngine _engine;
        static SwordDataTable _swordTable;

        static void Main(string[] args)
        {
            var csvContent = LoadCsv();
            var swords = new SwordDataLoader().Parse(csvContent);
            _swordTable = new SwordDataTable(swords);

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (args.Length > 0 && args[0] == "autotest")
            {
                RunAutoTest();
                return;
            }

            // Create engine
            var random = new SeededRandomProvider(Environment.TickCount);
            var time = new FakeTimeProvider();
            var context = new GameContext(random, time, _swordTable);
            _engine = GameEngine.Create(new PlayerData(), context);
            _engine.OnEvent += HandleEvent;

            Console.WriteLine("=== 검강화 게임 ===");
            Console.WriteLine();

            // Game loop
            while (true)
            {
                PrintStatus();
                PrintMenu();

                var key = Console.ReadKey(true).Key;
                Console.WriteLine();

                switch (key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        _engine.Dispatch(new EnhanceCommand());
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        _engine.Dispatch(new SellCommand());
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        _engine.Dispatch(new ConfirmDestroyCommand());
                        break;
                    case ConsoleKey.Q:
                        Console.WriteLine("게임을 종료합니다.");
                        return;
                    default:
                        Console.WriteLine("잘못된 입력입니다.");
                        break;
                }

                Console.WriteLine();
            }
        }

        static string LoadCsv()
        {
            var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "swords.csv");
            if (!File.Exists(csvPath))
                csvPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "swords.csv");
            if (!File.Exists(csvPath))
                csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Project", "Assets", "Resources", "Data", "swords.csv");
            return File.ReadAllText(csvPath);
        }

        static void RunAutoTest()
        {
            Console.WriteLine("=== 자동 테스트 모드 ===");
            Console.WriteLine();

            // Use fixed seed for deterministic results
            var random = new SeededRandomProvider(42);
            var time = new FakeTimeProvider();
            var context = new GameContext(random, time, _swordTable);
            _engine = GameEngine.Create(new PlayerData(), context);
            _engine.OnEvent += HandleEvent;

            Console.WriteLine($"초기 상태: 골드={_engine.State.PlayerData.Gold}G, 검={_engine.State.CurrentSword.Name}");
            Console.WriteLine();

            // Test 1: Enhance cycle
            Console.WriteLine("--- 강화 10회 시도 ---");
            for (int i = 0; i < 10; i++)
            {
                var state = _engine.State;
                if (state.PendingAdProtection)
                {
                    _engine.Dispatch(new ConfirmDestroyCommand());
                }
                _engine.Dispatch(new EnhanceCommand());
            }

            Console.WriteLine();
            Console.WriteLine($"결과: +{_engine.State.CurrentLevel} {_engine.State.CurrentSword.Name}, 골드={_engine.State.PlayerData.Gold}G");

            // Test 2: Sell if possible
            if (_engine.State.CurrentLevel > 0)
            {
                Console.WriteLine();
                Console.WriteLine("--- 판매 테스트 ---");
                _engine.Dispatch(new SellCommand());
                Console.WriteLine($"판매 후: 골드={_engine.State.PlayerData.Gold}G, 검={_engine.State.CurrentSword.Name}");
            }

            // Test 3: Reject cases
            Console.WriteLine();
            Console.WriteLine("--- 거부 테스트 ---");
            _engine.Dispatch(new SellCommand()); // Should reject: wooden sword
            _engine.Dispatch(new ConfirmDestroyCommand()); // Should reject: no pending

            // Test 4: Gold depletion
            Console.WriteLine();
            Console.WriteLine("--- 골드 소진까지 강화 ---");
            int attempts = 0;
            while (_engine.State.PlayerData.Gold > 0 && attempts < 200)
            {
                var state = _engine.State;
                if (state.PendingAdProtection)
                    _engine.Dispatch(new ConfirmDestroyCommand());

                if (_engine.State.PlayerData.Gold >= (_swordTable.GetSword(_engine.State.CurrentLevel + 1)?.EnhanceCost ?? int.MaxValue))
                    _engine.Dispatch(new EnhanceCommand());
                else
                    break;
                attempts++;
            }

            var final = _engine.State;
            Console.WriteLine();
            Console.WriteLine("=== 최종 결과 ===");
            Console.WriteLine($"  골드: {final.PlayerData.Gold}G");
            Console.WriteLine($"  검: +{final.CurrentLevel} {final.CurrentSword.Name}");
            Console.WriteLine($"  최고 기록: +{final.PlayerData.Stats.HighestEnhanceLevel}");
            Console.WriteLine($"  총 시도: {final.PlayerData.Stats.TotalEnhanceAttempts}회");
            Console.WriteLine($"  총 파괴: {final.PlayerData.Stats.TotalDestroys}회");
            Console.WriteLine($"  총 판매: {final.PlayerData.Stats.TotalSells}회");
            Console.WriteLine($"  최대 연속 성공: {final.PlayerData.Stats.MaxConsecutiveSuccess}회");
            Console.WriteLine($"  최대 연속 파괴: {final.PlayerData.Stats.MaxConsecutiveFail}회");
            // Test 5: Determinism - same seed = same result
            Console.WriteLine();
            Console.WriteLine("--- 결정론 테스트 (시드 123) ---");
            var random2 = new SeededRandomProvider(123);
            var context2 = new GameContext(random2, time, _swordTable);
            var engine2 = GameEngine.Create(new PlayerData(), context2);
            for (int i = 0; i < 20; i++)
            {
                if (engine2.State.PendingAdProtection)
                    engine2.Dispatch(new ConfirmDestroyCommand());
                var next = _swordTable.GetSword(engine2.State.CurrentLevel + 1);
                if (next != null && engine2.State.PlayerData.Gold >= next.EnhanceCost)
                    engine2.Dispatch(new EnhanceCommand());
                else
                    break;
            }
            var r1 = $"+{engine2.State.CurrentLevel} gold={engine2.State.PlayerData.Gold} destroys={engine2.State.PlayerData.Stats.TotalDestroys}";

            var random3 = new SeededRandomProvider(123);
            var context3 = new GameContext(random3, time, _swordTable);
            var engine3 = GameEngine.Create(new PlayerData(), context3);
            for (int i = 0; i < 20; i++)
            {
                if (engine3.State.PendingAdProtection)
                    engine3.Dispatch(new ConfirmDestroyCommand());
                var next = _swordTable.GetSword(engine3.State.CurrentLevel + 1);
                if (next != null && engine3.State.PlayerData.Gold >= next.EnhanceCost)
                    engine3.Dispatch(new EnhanceCommand());
                else
                    break;
            }
            var r2 = $"+{engine3.State.CurrentLevel} gold={engine3.State.PlayerData.Gold} destroys={engine3.State.PlayerData.Stats.TotalDestroys}";

            Console.WriteLine($"  실행1: {r1}");
            Console.WriteLine($"  실행2: {r2}");
            Console.WriteLine($"  결정론: {(r1 == r2 ? "OK ✓" : "FAIL ✕")}");

            Console.WriteLine();
            Console.WriteLine("=== 테스트 완료 ===");
        }

        static void PrintStatus()
        {
            var state = _engine.State;
            var sword = state.CurrentSword;
            var level = state.CurrentLevel;
            var gold = state.PlayerData.Gold;
            var stats = state.PlayerData.Stats;

            Console.WriteLine("─────────────────────────────────");
            var levelStr = level > 0 ? $"+{level} " : "";
            Console.WriteLine($"  검: {levelStr}{sword.Name} ({sword.Theme})");
            Console.WriteLine($"  골드: {gold:N0}G");

            if (level < _swordTable.MaxLevel)
            {
                var next = _swordTable.GetSword(level + 1);
                var rateStr = level + 1 <= 14 ? $"{next.SuccessRate * 100:F1}%" : "???";
                Console.WriteLine($"  다음 강화: {rateStr} / {next.EnhanceCost:N0}G");
            }
            else
            {
                Console.WriteLine("  *** 최고 단계 달성! ***");
            }

            if (level > 0)
            {
                Console.WriteLine($"  판매가: {sword.SellPrice:N0}G (회수율 {sword.ReturnRate * 100:F0}%)");
            }

            Console.WriteLine($"  통계: 최고+{stats.HighestEnhanceLevel} | 파괴 {stats.TotalDestroys}회 | 시도 {stats.TotalEnhanceAttempts}회");

            if (stats.CurrentConsecutiveSuccess > 1)
                Console.WriteLine($"  🔥 연속 성공 {stats.CurrentConsecutiveSuccess}회!");
            if (stats.CurrentConsecutiveFail > 1)
                Console.WriteLine($"  💀 연속 파괴 {stats.CurrentConsecutiveFail}회...");
        }

        static void PrintMenu()
        {
            var state = _engine.State;
            Console.WriteLine("─────────────────────────────────");

            if (state.PendingAdProtection)
            {
                Console.WriteLine("  [3] 파괴 확정 (나무검으로 리셋)");
            }
            else
            {
                if (state.CurrentLevel < _swordTable.MaxLevel)
                    Console.WriteLine("  [1] 강화");
                if (state.CurrentLevel > 0)
                    Console.WriteLine("  [2] 판매");
            }

            Console.WriteLine("  [Q] 종료");
            Console.Write("  > ");
        }

        static void HandleEvent(GameEvent evt)
        {
            switch (evt)
            {
                case EnhanceSuccessEvent e:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ★ 강화 성공! +{e.PrevLevel} → +{e.NewLevel} {e.NewSwordName}");
                    Console.ResetColor();
                    break;

                case EnhanceFailEvent e when e.Destroyed:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✕ 파괴! +{e.DestroyedLevel} {e.DestroyedSwordName}");
                    if (e.DestroyedLevel > 0)
                    {
                        var nextSword = _swordTable.GetSword(e.DestroyedLevel + 1);
                        if (nextSword != null)
                            Console.WriteLine($"    성공했다면 {nextSword.SellPrice:N0}G 였는데...");
                    }
                    Console.ResetColor();
                    break;

                case EnhanceFailEvent e when !e.Destroyed:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  ◆ 보호! 강화 실패했지만 파괴를 막았습니다.");
                    Console.ResetColor();
                    break;

                case SellEvent e:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"  $ 판매! +{e.SoldLevel} {e.SoldSwordName} → {e.GoldGained:N0}G 획득");
                    Console.ResetColor();
                    break;

                case DestroyConfirmedEvent e:
                    Console.WriteLine($"  나무검으로 돌아갑니다...");
                    break;

                case CommandRejectedEvent e:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    var reason = e.Reason switch
                    {
                        "insufficient_gold" => "골드가 부족합니다!",
                        "cannot_sell_wooden_sword" => "나무검은 판매할 수 없습니다.",
                        "max_level_reached" => "이미 최고 단계입니다!",
                        "pending_ad_protection" => "파괴 처리를 먼저 완료해주세요.",
                        "no_pending_destruction" => "파괴 대기 상태가 아닙니다.",
                        _ => e.Reason
                    };
                    Console.WriteLine($"  [{reason}]");
                    Console.ResetColor();
                    break;

                case GoldChangeEvent:
                    // Handled by status display
                    break;
            }
        }
    }
}
