using NUnit.Framework;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;
using System.Linq;

namespace GameCore.Tests.Logic
{
    [TestFixture]
    public class EconomyLogicTests
    {
        private EconomyLogic _logic;

        [SetUp]
        public void SetUp()
        {
            _logic = new EconomyLogic();
        }

        // --- CanAfford ---

        [Test]
        public void CanAfford_True_WhenGoldSufficient()
        {
            var state = TestHelper.CreateStateAtLevel(0, gold: 100);
            Assert.That(_logic.CanAfford(state, 100), Is.True);
        }

        [Test]
        public void CanAfford_False_WhenGoldInsufficient()
        {
            var state = TestHelper.CreateStateAtLevel(0, gold: 4);
            Assert.That(_logic.CanAfford(state, 5), Is.False);
        }

        [Test]
        public void CanAfford_True_WhenCostIsZero()
        {
            var state = TestHelper.CreateStateAtLevel(0, gold: 0);
            Assert.That(_logic.CanAfford(state, 0), Is.True);
        }

        // --- SpendGold ---

        [Test]
        public void SpendGold_DeductsCorrectAmount()
        {
            var state = TestHelper.CreateStateAtLevel(0, gold: 100);
            var result = _logic.SpendGold(state, 30);

            Assert.That(result.NewState.PlayerData.Gold, Is.EqualTo(70));
        }

        [Test]
        public void SpendGold_EmitsGoldChangeEvent()
        {
            var state = TestHelper.CreateStateAtLevel(0, gold: 100);
            var result = _logic.SpendGold(state, 30);

            var evt = result.Events.OfType<GoldChangeEvent>().Single();
            Assert.That(evt.Amount, Is.EqualTo(-30));
            Assert.That(evt.NewTotal, Is.EqualTo(70));
            Assert.That(evt.Reason, Is.EqualTo("enhance"));
        }

        // --- AddGold ---

        [Test]
        public void AddGold_AddsCorrectAmount()
        {
            var state = TestHelper.CreateStateAtLevel(0, gold: 50);
            var result = _logic.AddGold(state, 100, "sell");

            Assert.That(result.NewState.PlayerData.Gold, Is.EqualTo(150));
        }

        [Test]
        public void AddGold_UpdatesTotalGoldEarned()
        {
            var state = TestHelper.CreateStateAtLevel(0, gold: 50);
            var result = _logic.AddGold(state, 100, "sell");

            Assert.That(result.NewState.PlayerData.Stats.TotalGoldEarned, Is.EqualTo(100));
        }

        [Test]
        public void AddGold_AccumulatesTotalGoldEarned()
        {
            var state = TestHelper.CreateStateAtLevel(0, gold: 50);
            var stats = state.PlayerData.Stats.With(totalGoldEarned: 200);
            state = state.With(playerData: state.PlayerData.With(stats: stats));

            var result = _logic.AddGold(state, 100, "sell");

            Assert.That(result.NewState.PlayerData.Stats.TotalGoldEarned, Is.EqualTo(300));
        }

        [Test]
        public void AddGold_EmitsGoldChangeEvent()
        {
            var state = TestHelper.CreateStateAtLevel(0, gold: 50);
            var result = _logic.AddGold(state, 100, "sell");

            var evt = result.Events.OfType<GoldChangeEvent>().Single();
            Assert.That(evt.Amount, Is.EqualTo(100));
            Assert.That(evt.NewTotal, Is.EqualTo(150));
            Assert.That(evt.Reason, Is.EqualTo("sell"));
        }
    }
}
