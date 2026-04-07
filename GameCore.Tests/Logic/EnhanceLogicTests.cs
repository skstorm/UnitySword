using NUnit.Framework;
using GameCore.Commands;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;
using GameCore.Util;
using System.Linq;

namespace GameCore.Tests.Logic
{
    [TestFixture]
    public class EnhanceLogicTests
    {
        private EnhanceLogic _logic;
        private GameCore.Data.SwordDataTable _table;

        [SetUp]
        public void SetUp()
        {
            _logic = new EnhanceLogic();
            _table = TestHelper.CreateSmallTable();
        }

        // --- GetEffectiveRate ---

        [Test]
        public void GetEffectiveRate_ReturnsSwordSuccessRate()
        {
            var state = TestHelper.CreateStateAtLevel(0);
            var target = _table.GetSword(1); // 95%
            Assert.That(_logic.GetEffectiveRate(state, target), Is.EqualTo(0.95));
        }

        [Test]
        public void GetEffectiveRate_ClampsToZeroMin()
        {
            var state = TestHelper.CreateStateAtLevel(0);
            var badSword = new Sword(99, "X", "X", -0.5, 0, 0, 0, 0, 0, false);
            Assert.That(_logic.GetEffectiveRate(state, badSword), Is.EqualTo(0.0));
        }

        [Test]
        public void GetEffectiveRate_ClampsToOneMax()
        {
            var state = TestHelper.CreateStateAtLevel(0);
            var badSword = new Sword(99, "X", "X", 1.5, 0, 0, 0, 0, 0, false);
            Assert.That(_logic.GetEffectiveRate(state, badSword), Is.EqualTo(1.0));
        }

        // --- Roll ---

        [Test]
        public void Roll_SucceedsWhenRollBelowRate()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.5);
            Assert.That(_logic.Roll(0.95, random), Is.True);
        }

        [Test]
        public void Roll_FailsWhenRollAboveRate()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.96);
            Assert.That(_logic.Roll(0.95, random), Is.False);
        }

        [Test]
        public void Roll_FailsWhenRollExactlyEqualToRate()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.95);
            // NextDouble() < rate, so equal means fail
            Assert.That(_logic.Roll(0.95, random), Is.False);
        }

        [Test]
        public void Roll_AlwaysSucceedsWithRate1()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.999);
            Assert.That(_logic.Roll(1.0, random), Is.True);
        }

        [Test]
        public void Roll_AlwaysFailsWithRate0()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0);
            Assert.That(_logic.Roll(0.0, random), Is.False);
        }

        // --- HandleSuccess ---

        [Test]
        public void HandleSuccess_LevelsUp()
        {
            var state = TestHelper.CreateStateAtLevel(0);
            var target = _table.GetSword(1);
            var result = _logic.HandleSuccess(state, target, _table);

            Assert.That(result.NewState.CurrentLevel, Is.EqualTo(1));
            Assert.That(result.NewState.CurrentSword.Name, Is.EqualTo("철검"));
        }

        [Test]
        public void HandleSuccess_EmitsSuccessEvent()
        {
            var state = TestHelper.CreateStateAtLevel(2);
            var target = _table.GetSword(3);
            var result = _logic.HandleSuccess(state, target, _table);

            var evt = result.Events.OfType<EnhanceSuccessEvent>().Single();
            Assert.That(evt.PrevLevel, Is.EqualTo(2));
            Assert.That(evt.NewLevel, Is.EqualTo(3));
            Assert.That(evt.NewSwordName, Is.EqualTo("청동검"));
        }

        [Test]
        public void HandleSuccess_UpdatesStatistics()
        {
            var state = TestHelper.CreateStateAtLevel(0);
            var target = _table.GetSword(1);
            var result = _logic.HandleSuccess(state, target, _table);

            var stats = result.NewState.PlayerData.Stats;
            Assert.That(stats.TotalEnhanceAttempts, Is.EqualTo(1));
            Assert.That(stats.HighestEnhanceLevel, Is.EqualTo(1));
            Assert.That(stats.CurrentConsecutiveSuccess, Is.EqualTo(1));
            Assert.That(stats.CurrentConsecutiveFail, Is.EqualTo(0));
        }

        [Test]
        public void HandleSuccess_TracksHighestLevel()
        {
            // Start at level 3 with highest already 5
            var state = TestHelper.CreateStateAtLevel(3);
            var stats = state.PlayerData.Stats.With(highestEnhanceLevel: 5);
            state = state.With(playerData: state.PlayerData.With(stats: stats));

            var target = _table.GetSword(4);
            var result = _logic.HandleSuccess(state, target, _table);

            // Highest should remain 5, not become 4
            Assert.That(result.NewState.PlayerData.Stats.HighestEnhanceLevel, Is.EqualTo(5));
        }

        [Test]
        public void HandleSuccess_IncrementsConsecutiveSuccess()
        {
            var state = TestHelper.CreateStateAtLevel(1);
            var stats = state.PlayerData.Stats.With(currentConsecutiveSuccess: 3);
            state = state.With(playerData: state.PlayerData.With(stats: stats));

            var target = _table.GetSword(2);
            var result = _logic.HandleSuccess(state, target, _table);

            Assert.That(result.NewState.PlayerData.Stats.CurrentConsecutiveSuccess, Is.EqualTo(4));
        }

        [Test]
        public void HandleSuccess_ResetsConsecutiveFail()
        {
            var state = TestHelper.CreateStateAtLevel(1);
            var stats = state.PlayerData.Stats.With(currentConsecutiveFail: 5);
            state = state.With(playerData: state.PlayerData.With(stats: stats));

            var target = _table.GetSword(2);
            var result = _logic.HandleSuccess(state, target, _table);

            Assert.That(result.NewState.PlayerData.Stats.CurrentConsecutiveFail, Is.EqualTo(0));
        }

        // --- HandleFail ---

        [Test]
        public void HandleFail_IncrementsDestroyCount()
        {
            var state = TestHelper.CreateStateAtLevel(1);
            var ctx = TestHelper.CreateContext();
            var result = _logic.HandleFail(state, _table, ctx);

            Assert.That(result.NewState.PlayerData.Stats.TotalDestroys, Is.EqualTo(1));
        }

        [Test]
        public void HandleFail_EmitsFailEvent_Destroyed()
        {
            var state = TestHelper.CreateStateAtLevel(2);
            var ctx = TestHelper.CreateContext();
            var result = _logic.HandleFail(state, _table, ctx);

            var evt = result.Events.OfType<EnhanceFailEvent>().Single();
            Assert.That(evt.Destroyed, Is.True);
            Assert.That(evt.DestroyedLevel, Is.EqualTo(2));
            Assert.That(evt.DestroyedSwordName, Is.EqualTo("강철검"));
        }

        [Test]
        public void HandleFail_WithActiveProtection_DoesNotDestroy()
        {
            var state = TestHelper.CreateStateAtLevel(2)
                .With(hasActiveProtection: true);
            var ctx = TestHelper.CreateContext();
            var result = _logic.HandleFail(state, _table, ctx);

            var evt = result.Events.OfType<EnhanceFailEvent>().Single();
            Assert.That(evt.Destroyed, Is.False);
            Assert.That(result.NewState.HasActiveProtection, Is.False); // consumed
            Assert.That(result.NewState.CurrentLevel, Is.EqualTo(2)); // level preserved
        }

        [Test]
        public void HandleFail_SetsPendingAdProtection_WhenAdAvailable()
        {
            // Default AdLimits has AdProtectionUsedToday=0, so ad is available
            var state = TestHelper.CreateStateAtLevel(2);
            var ctx = TestHelper.CreateContext();
            var result = _logic.HandleFail(state, _table, ctx);

            Assert.That(result.NewState.PendingAdProtection, Is.True);
        }

        [Test]
        public void HandleFail_NoPendingAdProtection_WhenAdExhausted()
        {
            var state = TestHelper.CreateStateAtLevel(2);
            var adLimits = new AdLimits(adProtectionUsedToday: 2);
            state = state.With(playerData: state.PlayerData.With(adLimits: adLimits));

            var ctx = TestHelper.CreateContext();
            var result = _logic.HandleFail(state, _table, ctx);

            Assert.That(result.NewState.PendingAdProtection, Is.False);
        }

        [Test]
        public void HandleFail_ResetsConsecutiveSuccess()
        {
            var state = TestHelper.CreateStateAtLevel(1);
            var stats = state.PlayerData.Stats.With(currentConsecutiveSuccess: 3);
            state = state.With(playerData: state.PlayerData.With(stats: stats));

            var ctx = TestHelper.CreateContext();
            var result = _logic.HandleFail(state, _table, ctx);

            Assert.That(result.NewState.PlayerData.Stats.CurrentConsecutiveSuccess, Is.EqualTo(0));
        }

        [Test]
        public void HandleFail_IncrementsConsecutiveFail()
        {
            var state = TestHelper.CreateStateAtLevel(1);
            var stats = state.PlayerData.Stats.With(currentConsecutiveFail: 2);
            state = state.With(playerData: state.PlayerData.With(stats: stats));

            var ctx = TestHelper.CreateContext();
            var result = _logic.HandleFail(state, _table, ctx);

            Assert.That(result.NewState.PlayerData.Stats.CurrentConsecutiveFail, Is.EqualTo(3));
        }
    }
}
