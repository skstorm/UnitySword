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
            // 1. Load CSV data
            var csvContent = LoadCsv();
            var swords = new SwordDataLoader().Parse(csvContent);
            _swordTable = new SwordDataTable(swords);

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // 2. autotest branch (unchanged)
            if (args.Length > 0 && args[0] == "autotest")
            {
                RunAutoTest();
                return;
            }

            // 3. Load mastery data
            var masteryCsv = LoadMasteryCsv();
            MasteryDataTable masteryTable = null;
            if (masteryCsv != null)
            {
                var masteryLevels = new MasteryDataLoader().Parse(masteryCsv);
                masteryTable = new MasteryDataTable(masteryLevels);
            }

            // 4. Load save or create new
            var saveManager = new SaveManager();
            var playerData = saveManager.Load() ?? new PlayerData();

            // 5. Create engine with mastery table
            var random = new SeededRandomProvider(Environment.TickCount);
            var time = new FakeTimeProvider();
            var context = new GameContext(random, time, _swordTable, masteryTable);
            _engine = GameEngine.Create(playerData, context);

            // 6. Create renderer and menu handler
            var renderer = new ConsoleRenderer(_swordTable);
            var menuHandler = new MenuHandler(_engine, _swordTable, saveManager);
            _engine.OnEvent += renderer.HandleEvent;

            Console.WriteLine("=== 검강화 게임 ===");
            Console.WriteLine();

            // 7. Game loop
            while (true)
            {
                renderer.PrintStatus(_engine.State);
                menuHandler.PrintMenu(_engine.State);

                var key = Console.ReadKey(true).Key;
                Console.WriteLine();

                if (!menuHandler.HandleInput(key))
                    return;

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

        static string LoadMasteryCsv()
        {
            var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "mastery_levels.csv");
            if (!File.Exists(csvPath))
                csvPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "mastery_levels.csv");
            if (!File.Exists(csvPath))
                csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "doc", "mastery_levels.csv");
            if (!File.Exists(csvPath))
                return null;
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
            _engine.OnEvent += evt => { }; // Silent mode for autotest

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

    }
}
