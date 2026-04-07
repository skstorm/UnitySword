using NUnit.Framework;
using GameCore.Events;
using GameCore.Models;
using System.Linq;

namespace GameCore.Tests.Logic
{
    [TestFixture]
    public class FragmentLogicTests
    {
        private GameCore.Data.SwordDataTable _table;

        [SetUp]
        public void SetUp()
        {
            _table = TestHelper.CreateSmallTable();
        }

        [Test]
        public void GiveFragments_AddsBaseAmount()
        {
            var state = TestHelper.CreateStateAtLevel(3);

            var result = new GameCore.Logic.FragmentLogic().GiveFragments(state, 3, 0, _table);

            // Level 3 sword has FragmentReward=1
            Assert.That(result.NewState.PlayerData.Fragments, Is.EqualTo(1));
        }

        [Test]
        public void GiveFragments_AddsMasteryBonus()
        {
            var state = TestHelper.CreateStateAtLevel(3);

            var result = new GameCore.Logic.FragmentLogic().GiveFragments(state, 3, 2, _table);

            // base 1 + bonus 2 = 3
            Assert.That(result.NewState.PlayerData.Fragments, Is.EqualTo(3));
        }

        [Test]
        public void GiveFragments_AccumulatesWithExisting()
        {
            var state = TestHelper.CreateStateAtLevel(3);
            state = state.With(playerData: state.PlayerData.With(fragments: 10));

            var result = new GameCore.Logic.FragmentLogic().GiveFragments(state, 3, 0, _table);

            Assert.That(result.NewState.PlayerData.Fragments, Is.EqualTo(11));
        }

        [Test]
        public void GiveFragments_EmitsEvent()
        {
            var state = TestHelper.CreateStateAtLevel(3);
            state = state.With(playerData: state.PlayerData.With(fragments: 5));

            var result = new GameCore.Logic.FragmentLogic().GiveFragments(state, 3, 2, _table);

            var evt = result.Events.OfType<FragmentGainEvent>().Single();
            Assert.That(evt.BaseAmount, Is.EqualTo(1));
            Assert.That(evt.BonusAmount, Is.EqualTo(2));
            Assert.That(evt.TotalGained, Is.EqualTo(3));
            Assert.That(evt.NewTotal, Is.EqualTo(8));
            Assert.That(evt.FromLevel, Is.EqualTo(3));
        }

        [Test]
        public void GiveFragments_Level0_NoReward()
        {
            var state = TestHelper.CreateStateAtLevel(0);

            var result = new GameCore.Logic.FragmentLogic().GiveFragments(state, 0, 0, _table);

            Assert.That(result.NewState.PlayerData.Fragments, Is.EqualTo(0));
            Assert.That(result.Events, Is.Empty);
        }

        [Test]
        public void GiveFragments_InvalidLevel_NoChange()
        {
            var state = TestHelper.CreateStateAtLevel(1);

            var result = new GameCore.Logic.FragmentLogic().GiveFragments(state, 99, 0, _table);

            Assert.That(result.NewState.PlayerData.Fragments, Is.EqualTo(0));
            Assert.That(result.Events, Is.Empty);
        }

        [Test]
        public void GiveFragments_FullTable_HighLevel_MoreFragments()
        {
            var fullTable = TestHelper.LoadFullTable();
            var sword = fullTable.GetSword(15);
            var state = new GameState(sword, 15,
                new PlayerData(gold: 0, isFirstRun: false));

            var result = new GameCore.Logic.FragmentLogic().GiveFragments(state, 15, 1, fullTable);

            // Level 15 has FragmentReward=20, bonus=1 → 21
            Assert.That(result.NewState.PlayerData.Fragments, Is.EqualTo(21));
        }
    }
}
