using NUnit.Framework;
using GameCore.Events;
using GameCore.Models;
using System.Linq;

namespace GameCore.Tests.Logic
{
    [TestFixture]
    public class MasteryLogicTests
    {
        [Test]
        public void AddExp_IncrementsTotalAttempts()
        {
            var table = TestHelper.CreateSmallMasteryTable();
            var state = TestHelper.CreateStateAtLevel(1);

            var result = new GameCore.Logic.MasteryLogic().AddExp(state, table);

            Assert.That(result.NewState.PlayerData.Mastery.TotalAttempts, Is.EqualTo(1));
        }

        [Test]
        public void AddExp_EmitsMasteryExpEvent()
        {
            var table = TestHelper.CreateSmallMasteryTable();
            var state = TestHelper.CreateStateAtLevel(1);

            var result = new GameCore.Logic.MasteryLogic().AddExp(state, table);

            var evt = result.Events.OfType<MasteryExpEvent>().Single();
            Assert.That(evt.NewTotalAttempts, Is.EqualTo(1));
            Assert.That(evt.CurrentLevel, Is.EqualTo(1));
        }

        [Test]
        public void AddExp_LevelUp_WhenExpReached()
        {
            var table = TestHelper.CreateSmallMasteryTable(); // lv2 requires 3 exp
            var state = TestHelper.CreateStateAtLevel(1);
            // Set attempts to 2, so next +1 = 3 → level up to 2
            state = state.With(playerData: state.PlayerData.With(
                mastery: new MasteryData(level: 1, totalAttempts: 2)));

            var result = new GameCore.Logic.MasteryLogic().AddExp(state, table);

            Assert.That(result.NewState.PlayerData.Mastery.Level, Is.EqualTo(2));
            Assert.That(result.NewState.PlayerData.Mastery.TotalAttempts, Is.EqualTo(3));

            var levelUpEvt = result.Events.OfType<MasteryLevelUpEvent>().Single();
            Assert.That(levelUpEvt.PrevLevel, Is.EqualTo(1));
            Assert.That(levelUpEvt.NewLevel, Is.EqualTo(2));
        }

        [Test]
        public void AddExp_NoLevelUp_WhenExpNotReached()
        {
            var table = TestHelper.CreateSmallMasteryTable();
            var state = TestHelper.CreateStateAtLevel(1);
            state = state.With(playerData: state.PlayerData.With(
                mastery: new MasteryData(level: 1, totalAttempts: 1)));

            var result = new GameCore.Logic.MasteryLogic().AddExp(state, table);

            Assert.That(result.NewState.PlayerData.Mastery.Level, Is.EqualTo(1));
            Assert.That(result.Events.OfType<MasteryLevelUpEvent>().Count(), Is.EqualTo(0));
        }

        [Test]
        public void AddExp_CanSkipMultipleLevels()
        {
            var table = TestHelper.CreateSmallMasteryTable(); // lv2=3, lv3=8
            var state = TestHelper.CreateStateAtLevel(1);
            // Set attempts to 7, so next +1 = 8 → skip to level 3
            state = state.With(playerData: state.PlayerData.With(
                mastery: new MasteryData(level: 1, totalAttempts: 7)));

            var result = new GameCore.Logic.MasteryLogic().AddExp(state, table);

            Assert.That(result.NewState.PlayerData.Mastery.Level, Is.EqualTo(3));
            var levelUpEvt = result.Events.OfType<MasteryLevelUpEvent>().Single();
            Assert.That(levelUpEvt.NewLevel, Is.EqualTo(3));
        }

        [Test]
        public void AddExp_DoesNotExceedMaxLevel()
        {
            var table = TestHelper.CreateSmallMasteryTable(); // max=3
            var state = TestHelper.CreateStateAtLevel(1);
            state = state.With(playerData: state.PlayerData.With(
                mastery: new MasteryData(level: 3, totalAttempts: 100)));

            var result = new GameCore.Logic.MasteryLogic().AddExp(state, table);

            Assert.That(result.NewState.PlayerData.Mastery.Level, Is.EqualTo(3));
            Assert.That(result.NewState.PlayerData.Mastery.TotalAttempts, Is.EqualTo(101));
        }

        [Test]
        public void GetCostDiscount_ReturnsCorrectValue()
        {
            var table = TestHelper.CreateSmallMasteryTable();
            var state = TestHelper.CreateStateAtLevel(1);
            state = state.With(playerData: state.PlayerData.With(
                mastery: new MasteryData(level: 2)));

            var discount = new GameCore.Logic.MasteryLogic().GetCostDiscount(state, table);
            Assert.That(discount, Is.EqualTo(0.05));
        }

        [Test]
        public void GetCostDiscount_NullTable_ReturnsZero()
        {
            var state = TestHelper.CreateStateAtLevel(1);
            var discount = new GameCore.Logic.MasteryLogic().GetCostDiscount(state, null);
            Assert.That(discount, Is.EqualTo(0));
        }

        [Test]
        public void GetFragmentBonus_ReturnsCorrectValue()
        {
            var table = TestHelper.CreateSmallMasteryTable(); // lv3 has fragmentBonus=1
            var state = TestHelper.CreateStateAtLevel(1);
            state = state.With(playerData: state.PlayerData.With(
                mastery: new MasteryData(level: 3)));

            var bonus = new GameCore.Logic.MasteryLogic().GetFragmentBonus(state, table);
            Assert.That(bonus, Is.EqualTo(1));
        }
    }
}
