using GameCore.Commands;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Models;
using GameCore.Util;

namespace GameCore.Tests
{
    internal static class TestHelper
    {
        /// <summary>
        /// Creates a minimal sword table with levels 0~5 for fast tests.
        /// </summary>
        public static SwordDataTable CreateSmallTable()
        {
            var swords = new[]
            {
                new Sword(0, "나무검", "기본", 0, 0, 0, 0, 0, 0, false),
                new Sword(1, "철검", "기본", 0.95, 5, 5, 4, 0.70, 1, false),
                new Sword(2, "강철검", "기본", 0.92, 8, 13, 10, 0.75, 1, false),
                new Sword(3, "청동검", "기본", 0.88, 12, 25, 19, 0.76, 1, false),
                new Sword(4, "백은검", "기본", 0.83, 20, 45, 34, 0.78, 1, false),
                new Sword(5, "황금검", "기본", 0.77, 30, 75, 60, 0.80, 1, false),
            };
            return new SwordDataTable(swords);
        }

        /// <summary>
        /// Loads the full sword table from the real CSV.
        /// </summary>
        public static SwordDataTable LoadFullTable()
        {
            var csvPath = System.IO.Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory, "Data", "swords.csv");
            var csv = System.IO.File.ReadAllText(csvPath);
            return new SwordDataTable(new SwordDataLoader().Parse(csv));
        }

        public static GameContext CreateContext(
            FakeRandomProvider random = null,
            SwordDataTable table = null)
        {
            return new GameContext(
                random ?? new FakeRandomProvider(),
                new FakeTimeProvider(),
                table ?? CreateSmallTable());
        }

        /// <summary>
        /// Creates an engine at the given level with the specified gold.
        /// FakeRandom default=0.0 (always succeed) to pump the level up,
        /// then replaces it with the provided random for the actual test.
        /// </summary>
        public static GameEngine CreateEngineAtLevel(
            int level, int gold = 200,
            FakeRandomProvider random = null,
            SwordDataTable table = null)
        {
            table ??= CreateSmallTable();
            var pumpRandom = new FakeRandomProvider(0.0); // always succeed
            var ctx = new GameContext(pumpRandom, new FakeTimeProvider(), table);
            var player = new PlayerData(gold: 99999, isFirstRun: false);
            var engine = GameEngine.Create(player, ctx);

            for (int i = 0; i < level; i++)
                engine.Dispatch(new EnhanceCommand());

            // Now swap to the real random and set desired gold
            var finalRandom = random ?? new FakeRandomProvider();
            var finalCtx = new GameContext(finalRandom, new FakeTimeProvider(), table);
            var state = engine.State.With(
                playerData: engine.State.PlayerData.With(gold: gold));
            return new GameEngine(state, finalCtx);
        }

        public static GameState CreateStateAtLevel(int level, int gold = 200, SwordDataTable table = null)
        {
            table ??= CreateSmallTable();
            var sword = table.GetSword(level);
            return new GameState(
                currentSword: sword,
                currentLevel: level,
                playerData: new PlayerData(gold: gold, isFirstRun: false));
        }
    }
}
