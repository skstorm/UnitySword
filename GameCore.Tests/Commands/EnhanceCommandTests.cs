using NUnit.Framework;
using GameCore.Commands;
using GameCore.Events;
using GameCore.Models;
using GameCore.Util;
using System.Linq;

namespace GameCore.Tests.Commands
{
    [TestFixture]
    public class EnhanceCommandTests
    {
        private GameCore.Data.SwordDataTable _table;

        [SetUp]
        public void SetUp()
        {
            _table = TestHelper.CreateSmallTable();
        }

        // --- Validate ---

        [Test]
        public void Validate_RejectsPendingAdProtection()
        {
            var state = TestHelper.CreateStateAtLevel(1)
                .With(pendingAdProtection: true);
            var ctx = TestHelper.CreateContext(table: _table);
            var cmd = new EnhanceCommand();

            Assert.That(cmd.Validate(state, ctx), Is.EqualTo("pending_ad_protection"));
        }

        [Test]
        public void Validate_RejectsMaxLevel()
        {
            var state = TestHelper.CreateStateAtLevel(5); // max in small table
            var ctx = TestHelper.CreateContext(table: _table);
            var cmd = new EnhanceCommand();

            Assert.That(cmd.Validate(state, ctx), Is.EqualTo("max_level_reached"));
        }

        [Test]
        public void Validate_RejectsInsufficientGold()
        {
            var state = TestHelper.CreateStateAtLevel(0, gold: 2); // needs 5
            var ctx = TestHelper.CreateContext(table: _table);
            var cmd = new EnhanceCommand();

            Assert.That(cmd.Validate(state, ctx), Is.EqualTo("insufficient_gold"));
        }

        [Test]
        public void Validate_AcceptsValidEnhance()
        {
            var state = TestHelper.CreateStateAtLevel(0, gold: 100);
            var ctx = TestHelper.CreateContext(table: _table);
            var cmd = new EnhanceCommand();

            Assert.That(cmd.Validate(state, ctx), Is.Null);
        }

        // --- Execute: Success path ---

        [Test]
        public void Execute_Success_LevelsUpAndDeductsGold()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0); // always succeed
            var state = TestHelper.CreateStateAtLevel(0, gold: 100);
            var ctx = TestHelper.CreateContext(random: random, table: _table);
            var cmd = new EnhanceCommand();

            var result = cmd.Execute(state, ctx);

            Assert.That(result.NewState.CurrentLevel, Is.EqualTo(1));
            Assert.That(result.NewState.PlayerData.Gold, Is.EqualTo(95)); // 100 - 5
        }

        [Test]
        public void Execute_Success_EmitsGoldChangeAndSuccessEvents()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0);
            var state = TestHelper.CreateStateAtLevel(0, gold: 100);
            var ctx = TestHelper.CreateContext(random: random, table: _table);
            var cmd = new EnhanceCommand();

            var result = cmd.Execute(state, ctx);

            Assert.That(result.Events.OfType<GoldChangeEvent>().Count(), Is.EqualTo(1));
            Assert.That(result.Events.OfType<EnhanceSuccessEvent>().Count(), Is.EqualTo(1));

            var success = result.Events.OfType<EnhanceSuccessEvent>().Single();
            Assert.That(success.PrevLevel, Is.EqualTo(0));
            Assert.That(success.NewLevel, Is.EqualTo(1));
        }

        // --- Execute: Fail path ---

        [Test]
        public void Execute_Fail_WithAdAvailable_SetsPending()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.999); // fail
            var state = TestHelper.CreateStateAtLevel(1, gold: 100);
            var ctx = TestHelper.CreateContext(random: random, table: _table);
            var cmd = new EnhanceCommand();

            var result = cmd.Execute(state, ctx);

            // Ad is available (default AdLimits has 0 used), so pending
            Assert.That(result.NewState.PendingAdProtection, Is.True);
            // Level preserved until ConfirmDestroy
            Assert.That(result.NewState.CurrentLevel, Is.EqualTo(1));
        }

        [Test]
        public void Execute_Fail_WithAdExhausted_DestroysImmediately()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.999); // fail
            var state = TestHelper.CreateStateAtLevel(2, gold: 100);
            var adLimits = new AdLimits(adProtectionUsedToday: 2);
            state = state.With(playerData: state.PlayerData.With(adLimits: adLimits));
            var ctx = TestHelper.CreateContext(random: random, table: _table);
            var cmd = new EnhanceCommand();

            var result = cmd.Execute(state, ctx);

            Assert.That(result.NewState.PendingAdProtection, Is.False);
            Assert.That(result.NewState.CurrentLevel, Is.EqualTo(0));
            Assert.That(result.NewState.CurrentSword.Name, Is.EqualTo("나무검"));
        }

        [Test]
        public void Execute_Fail_WithAdExhausted_EmitsDestroyConfirmedEvent()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.999);
            var state = TestHelper.CreateStateAtLevel(3, gold: 100);
            var adLimits = new AdLimits(adProtectionUsedToday: 2);
            state = state.With(playerData: state.PlayerData.With(adLimits: adLimits));
            var ctx = TestHelper.CreateContext(random: random, table: _table);
            var cmd = new EnhanceCommand();

            var result = cmd.Execute(state, ctx);

            var destroyEvt = result.Events.OfType<DestroyConfirmedEvent>().Single();
            Assert.That(destroyEvt.DestroyedLevel, Is.EqualTo(3));
        }

        [Test]
        public void Execute_Fail_WithActiveProtection_PreservesLevel()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.999); // fail
            var state = TestHelper.CreateStateAtLevel(3, gold: 100)
                .With(hasActiveProtection: true);
            var ctx = TestHelper.CreateContext(random: random, table: _table);
            var cmd = new EnhanceCommand();

            var result = cmd.Execute(state, ctx);

            Assert.That(result.NewState.CurrentLevel, Is.EqualTo(3));
            Assert.That(result.NewState.HasActiveProtection, Is.False);

            var failEvt = result.Events.OfType<EnhanceFailEvent>().Single();
            Assert.That(failEvt.Destroyed, Is.False);
        }

        [Test]
        public void Execute_DeductsCorrectCostPerLevel()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0); // succeed
            // Level 2 → 3 costs 12
            var state = TestHelper.CreateStateAtLevel(2, gold: 100);
            var ctx = TestHelper.CreateContext(random: random, table: _table);
            var cmd = new EnhanceCommand();

            var result = cmd.Execute(state, ctx);

            Assert.That(result.NewState.PlayerData.Gold, Is.EqualTo(88)); // 100 - 12
        }

        [Test]
        public void Execute_IncrementsEnhanceAttempts()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0);
            var state = TestHelper.CreateStateAtLevel(0, gold: 100);
            var ctx = TestHelper.CreateContext(random: random, table: _table);
            var cmd = new EnhanceCommand();

            var result = cmd.Execute(state, ctx);

            Assert.That(result.NewState.PlayerData.Stats.TotalEnhanceAttempts, Is.EqualTo(1));
        }
    }
}
