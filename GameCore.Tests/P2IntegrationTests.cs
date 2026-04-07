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
    public class P2IntegrationTests
    {
        private GameCore.Data.SwordDataTable _table;
        private GameCore.Data.MasteryDataTable _masteryTable;

        [SetUp]
        public void SetUp()
        {
            _table = TestHelper.CreateSmallTable();
            _masteryTable = TestHelper.CreateSmallMasteryTable();
        }

        [Test]
        public void Enhance_WithMastery_GainsExp()
        {
            var random = new FakeRandomProvider(0.0);
            var ctx = new GameContext(random, new FakeTimeProvider(), _table, _masteryTable);
            var engine = GameEngine.Create(new PlayerData(), ctx);

            engine.Dispatch(new EnhanceCommand());

            Assert.That(engine.State.PlayerData.Mastery.TotalAttempts, Is.EqualTo(1));
        }

        [Test]
        public void Enhance_WithMastery_CostDiscount()
        {
            var random = new FakeRandomProvider(0.0);
            var ctx = new GameContext(random, new FakeTimeProvider(), _table, _masteryTable);
            // Start at mastery level 2 (5% discount)
            var player = new PlayerData(gold: 200, isFirstRun: false,
                mastery: new MasteryData(level: 2, totalAttempts: 50));
            var engine = GameEngine.Create(player, ctx);

            engine.Dispatch(new EnhanceCommand()); // +0→+1, base cost 5, 5% off = 4

            // 200 - 4 = 196
            Assert.That(engine.State.PlayerData.Gold, Is.EqualTo(196));
        }

        [Test]
        public void Enhance_Fail_GivesFragments()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0); // succeed +0→+1
            random.SetNext(0.999); // fail +1→+2
            var adLimits = new AdLimits(adProtectionUsedToday: 2); // no ad
            var ctx = new GameContext(random, new FakeTimeProvider(), _table, _masteryTable);
            var player = new PlayerData(gold: 9999, isFirstRun: false, adLimits: adLimits);
            var engine = GameEngine.Create(player, ctx);

            engine.Dispatch(new EnhanceCommand()); // succeed
            engine.Dispatch(new EnhanceCommand()); // fail, destroy → fragments

            // Level 1 sword has FragmentReward=1, mastery lv1 bonus=0 → 1 fragment
            Assert.That(engine.State.PlayerData.Fragments, Is.EqualTo(1));
            Assert.That(engine.State.CurrentLevel, Is.EqualTo(0));
        }

        [Test]
        public void ConfirmDestroy_GivesFragments()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0); // succeed
            random.SetNext(0.999); // fail, pending ad
            var ctx = new GameContext(random, new FakeTimeProvider(), _table, _masteryTable);
            var engine = GameEngine.Create(new PlayerData(), ctx);

            engine.Dispatch(new EnhanceCommand()); // succeed +0→+1
            engine.Dispatch(new EnhanceCommand()); // fail, pending ad

            Assert.That(engine.State.PendingAdProtection, Is.True);

            engine.Dispatch(new ConfirmDestroyCommand());

            Assert.That(engine.State.PlayerData.Fragments, Is.EqualTo(1));
            Assert.That(engine.State.CurrentLevel, Is.EqualTo(0));
        }

        [Test]
        public void Enhance_MasteryLevelUp_DuringPlay()
        {
            var random = new FakeRandomProvider(0.0); // always succeed
            var ctx = new GameContext(random, new FakeTimeProvider(), _table, _masteryTable);
            // Start with 2 attempts (need 3 for lv2)
            var player = new PlayerData(gold: 9999, isFirstRun: false,
                mastery: new MasteryData(level: 1, totalAttempts: 2));
            var engine = GameEngine.Create(player, ctx);

            var events = new List<GameEvent>();
            engine.OnEvent += e => events.Add(e);

            engine.Dispatch(new EnhanceCommand()); // 3rd attempt → level up

            Assert.That(engine.State.PlayerData.Mastery.Level, Is.EqualTo(2));
            Assert.That(events.OfType<MasteryLevelUpEvent>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Exchange_ThenUseAmulet_ThenEnhanceFail_PreservesLevel()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0); // succeed +0→+1
            random.SetNext(0.0); // succeed +1→+2
            random.SetNext(0.999); // fail +2→+3
            var ctx = new GameContext(random, new FakeTimeProvider(), _table, _masteryTable);
            var player = new PlayerData(gold: 9999, isFirstRun: false, fragments: 30);
            var engine = GameEngine.Create(player, ctx);

            // Exchange fragments for amulet
            engine.Dispatch(new ExchangeCommand("protection_amulet"));
            Assert.That(engine.State.PlayerData.Fragments, Is.EqualTo(0));
            Assert.That(engine.State.PlayerData.Inventory.ProtectionAmulets, Is.EqualTo(1));

            // Enhance to +2
            engine.Dispatch(new EnhanceCommand());
            engine.Dispatch(new EnhanceCommand());
            Assert.That(engine.State.CurrentLevel, Is.EqualTo(2));

            // Use amulet
            engine.Dispatch(new UseItemCommand());
            Assert.That(engine.State.HasActiveProtection, Is.True);

            // Fail → protection absorbs
            engine.Dispatch(new EnhanceCommand());
            Assert.That(engine.State.CurrentLevel, Is.EqualTo(2)); // preserved!
            Assert.That(engine.State.HasActiveProtection, Is.False);
        }

        [Test]
        public void Exchange_GoldPouch_AddsGold()
        {
            var ctx = new GameContext(new FakeRandomProvider(), new FakeTimeProvider(), _table, _masteryTable);
            var player = new PlayerData(gold: 100, isFirstRun: false, fragments: 20);
            var engine = GameEngine.Create(player, ctx);

            engine.Dispatch(new ExchangeCommand("gold_pouch"));

            Assert.That(engine.State.PlayerData.Gold, Is.EqualTo(600)); // 100+500
            Assert.That(engine.State.PlayerData.Fragments, Is.EqualTo(10));
        }

        [Test]
        public void Collect_ThenResetToWoodenSword()
        {
            var fullTable = TestHelper.LoadFullTable();
            var random = new FakeRandomProvider(0.0); // always succeed
            var ctx = new GameContext(random, new FakeTimeProvider(), fullTable, _masteryTable);
            var player = new PlayerData(gold: 999999, isFirstRun: false);
            var engine = GameEngine.Create(player, ctx);

            // Enhance to +10
            for (int i = 0; i < 10; i++)
                engine.Dispatch(new EnhanceCommand());

            Assert.That(engine.State.CurrentLevel, Is.EqualTo(10));

            // Collect
            engine.Dispatch(new CollectCommand());

            Assert.That(engine.State.CurrentLevel, Is.EqualTo(0));
            Assert.That(engine.State.PlayerData.Collection.Collected.ContainsKey(10), Is.True);
        }

        [Test]
        public void Enhance_WithoutMasteryTable_StillWorks()
        {
            // Backward compatibility: P1 tests pass without mastery table
            var random = new FakeRandomProvider(0.0);
            var ctx = new GameContext(random, new FakeTimeProvider(), _table);
            var engine = GameEngine.Create(new PlayerData(), ctx);

            engine.Dispatch(new EnhanceCommand());

            Assert.That(engine.State.CurrentLevel, Is.EqualTo(1));
            Assert.That(engine.State.PlayerData.Gold, Is.EqualTo(195));
        }

        [Test]
        public void Enhance_Fail_WithMasteryBonus_GivesExtraFragments()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0); // succeed
            random.SetNext(0.999); // fail
            var adLimits = new AdLimits(adProtectionUsedToday: 2);
            var ctx = new GameContext(random, new FakeTimeProvider(), _table, _masteryTable);
            // Mastery level 3 has fragmentBonus=1
            var player = new PlayerData(gold: 9999, isFirstRun: false, adLimits: adLimits,
                mastery: new MasteryData(level: 3, totalAttempts: 100));
            var engine = GameEngine.Create(player, ctx);

            engine.Dispatch(new EnhanceCommand()); // succeed
            engine.Dispatch(new EnhanceCommand()); // fail → fragments

            // Level 1: base 1 + mastery bonus 1 = 2
            Assert.That(engine.State.PlayerData.Fragments, Is.EqualTo(2));
        }
    }
}
