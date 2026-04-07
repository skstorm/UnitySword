using NUnit.Framework;
using GameCore.Commands;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;
using GameCore.Util;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private GameCore.Data.SwordDataTable _table;

        [SetUp]
        public void SetUp()
        {
            _table = TestHelper.CreateSmallTable();
        }

        // --- Full game cycle ---

        [Test]
        public void FullCycle_Enhance_Sell_ResetToWoodenSword()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0); // succeed
            var ctx = new GameContext(random, new FakeTimeProvider(), _table);
            var engine = GameEngine.Create(new PlayerData(), ctx);

            // Enhance +0 → +1
            engine.Dispatch(new EnhanceCommand());
            Assert.That(engine.State.CurrentLevel, Is.EqualTo(1));
            Assert.That(engine.State.PlayerData.Gold, Is.EqualTo(195)); // 200 - 5

            // Sell +1
            engine.Dispatch(new SellCommand());
            Assert.That(engine.State.CurrentLevel, Is.EqualTo(0));
            Assert.That(engine.State.PlayerData.Gold, Is.EqualTo(199)); // 195 + 4
            Assert.That(engine.State.CurrentSword.Name, Is.EqualTo("나무검"));
        }

        [Test]
        public void FullCycle_Enhance_Destroy_ConfirmDestroy()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0); // first: succeed
            random.SetNext(0.999); // second: fail
            var ctx = new GameContext(random, new FakeTimeProvider(), _table);
            var engine = GameEngine.Create(new PlayerData(), ctx);

            // Enhance +0 → +1 (success)
            engine.Dispatch(new EnhanceCommand());
            Assert.That(engine.State.CurrentLevel, Is.EqualTo(1));

            // Enhance +1 → +2 (fail, pending ad)
            engine.Dispatch(new EnhanceCommand());
            Assert.That(engine.State.PendingAdProtection, Is.True);

            // Can't enhance or sell while pending
            var events = new List<GameEvent>();
            engine.OnEvent += e => events.Add(e);

            engine.Dispatch(new EnhanceCommand());
            Assert.That(events.Last(), Is.InstanceOf<CommandRejectedEvent>());

            engine.Dispatch(new SellCommand());
            Assert.That(events.Last(), Is.InstanceOf<CommandRejectedEvent>());

            // Confirm destroy
            engine.Dispatch(new ConfirmDestroyCommand());
            Assert.That(engine.State.CurrentLevel, Is.EqualTo(0));
            Assert.That(engine.State.PendingAdProtection, Is.False);
        }

        [Test]
        public void FullCycle_EnhanceMultipleTimes_StatsAccumulate()
        {
            var random = new FakeRandomProvider(0.0); // always succeed
            var ctx = new GameContext(random, new FakeTimeProvider(), _table);
            var engine = GameEngine.Create(new PlayerData(), ctx);

            // Enhance 3 times: +0→+1→+2→+3
            engine.Dispatch(new EnhanceCommand());
            engine.Dispatch(new EnhanceCommand());
            engine.Dispatch(new EnhanceCommand());

            var stats = engine.State.PlayerData.Stats;
            Assert.That(stats.TotalEnhanceAttempts, Is.EqualTo(3));
            Assert.That(stats.HighestEnhanceLevel, Is.EqualTo(3));
            Assert.That(stats.CurrentConsecutiveSuccess, Is.EqualTo(3));
            Assert.That(stats.MaxConsecutiveSuccess, Is.EqualTo(3));
        }

        // --- Determinism ---

        [Test]
        public void Determinism_SameSeed_SameResult()
        {
            var fullTable = TestHelper.LoadFullTable();

            string RunSimulation(int seed)
            {
                var random = new SeededRandomProvider(seed);
                var ctx = new GameContext(random, new FakeTimeProvider(), fullTable);
                var engine = GameEngine.Create(new PlayerData(), ctx);

                for (int i = 0; i < 50; i++)
                {
                    if (engine.State.PendingAdProtection)
                        engine.Dispatch(new ConfirmDestroyCommand());

                    var next = fullTable.GetSword(engine.State.CurrentLevel + 1);
                    if (next != null && engine.State.PlayerData.Gold >= next.EnhanceCost)
                        engine.Dispatch(new EnhanceCommand());
                    else
                        break;
                }

                var s = engine.State;
                return $"{s.CurrentLevel},{s.PlayerData.Gold},{s.PlayerData.Stats.TotalDestroys}";
            }

            var run1 = RunSimulation(42);
            var run2 = RunSimulation(42);

            Assert.That(run1, Is.EqualTo(run2));
        }

        [Test]
        public void Determinism_DifferentSeed_DifferentResult()
        {
            var fullTable = TestHelper.LoadFullTable();

            string RunSimulation(int seed)
            {
                var random = new SeededRandomProvider(seed);
                var ctx = new GameContext(random, new FakeTimeProvider(), fullTable);
                var engine = GameEngine.Create(new PlayerData(), ctx);

                for (int i = 0; i < 30; i++)
                {
                    if (engine.State.PendingAdProtection)
                        engine.Dispatch(new ConfirmDestroyCommand());

                    var next = fullTable.GetSword(engine.State.CurrentLevel + 1);
                    if (next != null && engine.State.PlayerData.Gold >= next.EnhanceCost)
                        engine.Dispatch(new EnhanceCommand());
                    else
                        break;
                }

                var s = engine.State;
                return $"{s.CurrentLevel},{s.PlayerData.Gold},{s.PlayerData.Stats.TotalDestroys}";
            }

            var run1 = RunSimulation(42);
            var run2 = RunSimulation(999);

            // Very unlikely to be the same with different seeds
            Assert.That(run1, Is.Not.EqualTo(run2));
        }

        // --- Consecutive tracking across success/fail transitions ---

        [Test]
        public void ConsecutiveStats_ResetOnTransition()
        {
            var random = new FakeRandomProvider();
            var adLimits = new AdLimits(adProtectionUsedToday: 2); // no ad, immediate destroy
            var ctx = new GameContext(random, new FakeTimeProvider(), _table);
            var player = new PlayerData(gold: 9999, isFirstRun: false, adLimits: adLimits);
            var engine = GameEngine.Create(player, ctx);

            // 3 successes
            random.SetNext(0.0);
            random.SetNext(0.0);
            random.SetNext(0.0);
            engine.Dispatch(new EnhanceCommand());
            engine.Dispatch(new EnhanceCommand());
            engine.Dispatch(new EnhanceCommand());

            Assert.That(engine.State.PlayerData.Stats.CurrentConsecutiveSuccess, Is.EqualTo(3));
            Assert.That(engine.State.PlayerData.Stats.CurrentConsecutiveFail, Is.EqualTo(0));

            // 1 fail (destroys, back to +0)
            random.SetNext(0.999);
            engine.Dispatch(new EnhanceCommand());

            Assert.That(engine.State.PlayerData.Stats.CurrentConsecutiveSuccess, Is.EqualTo(0));
            Assert.That(engine.State.PlayerData.Stats.CurrentConsecutiveFail, Is.EqualTo(1));
            Assert.That(engine.State.PlayerData.Stats.MaxConsecutiveSuccess, Is.EqualTo(3));

            // 2 more successes
            random.SetNext(0.0);
            random.SetNext(0.0);
            engine.Dispatch(new EnhanceCommand());
            engine.Dispatch(new EnhanceCommand());

            Assert.That(engine.State.PlayerData.Stats.CurrentConsecutiveSuccess, Is.EqualTo(2));
            Assert.That(engine.State.PlayerData.Stats.CurrentConsecutiveFail, Is.EqualTo(0));
            Assert.That(engine.State.PlayerData.Stats.MaxConsecutiveSuccess, Is.EqualTo(3)); // still 3
        }

        // --- Edge cases ---

        [Test]
        public void EdgeCase_EnhanceWithExactGold()
        {
            // Level 0→1 costs 5, give exactly 5
            var random = new FakeRandomProvider();
            random.SetNext(0.0);
            var ctx = new GameContext(random, new FakeTimeProvider(), _table);
            var player = new PlayerData(gold: 5, isFirstRun: false);
            var engine = GameEngine.Create(player, ctx);

            engine.Dispatch(new EnhanceCommand());

            Assert.That(engine.State.CurrentLevel, Is.EqualTo(1));
            Assert.That(engine.State.PlayerData.Gold, Is.EqualTo(0));
        }

        [Test]
        public void EdgeCase_SellAfterMultipleEnhances()
        {
            var random = new FakeRandomProvider(0.0);
            var ctx = new GameContext(random, new FakeTimeProvider(), _table);
            var engine = GameEngine.Create(new PlayerData(), ctx);

            // Enhance to +5
            for (int i = 0; i < 5; i++)
                engine.Dispatch(new EnhanceCommand());

            Assert.That(engine.State.CurrentLevel, Is.EqualTo(5));

            // Sell +5 (sell price = 60)
            engine.Dispatch(new SellCommand());
            var expectedGold = 200 - 5 - 8 - 12 - 20 - 30 + 60;
            Assert.That(engine.State.PlayerData.Gold, Is.EqualTo(expectedGold));
            Assert.That(engine.State.CurrentLevel, Is.EqualTo(0));
        }

        [Test]
        public void EdgeCase_CannotEnhanceAtMaxLevel()
        {
            // Small table max is 5
            var random = new FakeRandomProvider(0.0);
            var ctx = new GameContext(random, new FakeTimeProvider(), _table);
            var engine = GameEngine.Create(new PlayerData(gold: 9999), ctx);

            for (int i = 0; i < 5; i++)
                engine.Dispatch(new EnhanceCommand());

            Assert.That(engine.State.CurrentLevel, Is.EqualTo(5));

            var events = new List<GameEvent>();
            engine.OnEvent += e => events.Add(e);
            engine.Dispatch(new EnhanceCommand());

            var rejected = events.OfType<CommandRejectedEvent>().Single();
            Assert.That(rejected.Reason, Is.EqualTo("max_level_reached"));
        }
    }
}
