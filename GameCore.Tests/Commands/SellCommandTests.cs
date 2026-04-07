using NUnit.Framework;
using GameCore.Commands;
using GameCore.Events;
using GameCore.Models;
using System.Linq;

namespace GameCore.Tests.Commands
{
    [TestFixture]
    public class SellCommandTests
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

            Assert.That(new SellCommand().Validate(state, ctx),
                Is.EqualTo("pending_ad_protection"));
        }

        [Test]
        public void Validate_RejectsWoodenSword()
        {
            var state = TestHelper.CreateStateAtLevel(0);
            var ctx = TestHelper.CreateContext(table: _table);

            Assert.That(new SellCommand().Validate(state, ctx),
                Is.EqualTo("cannot_sell_wooden_sword"));
        }

        [Test]
        public void Validate_AcceptsEnhancedSword()
        {
            var state = TestHelper.CreateStateAtLevel(1);
            var ctx = TestHelper.CreateContext(table: _table);

            Assert.That(new SellCommand().Validate(state, ctx), Is.Null);
        }

        // --- Execute ---

        [Test]
        public void Execute_AddsCorrectGold()
        {
            // +1 철검 sell price = 4
            var state = TestHelper.CreateStateAtLevel(1, gold: 50);
            var ctx = TestHelper.CreateContext(table: _table);

            var result = new SellCommand().Execute(state, ctx);

            Assert.That(result.NewState.PlayerData.Gold, Is.EqualTo(54));
        }

        [Test]
        public void Execute_ResetsToWoodenSword()
        {
            var state = TestHelper.CreateStateAtLevel(3, gold: 50);
            var ctx = TestHelper.CreateContext(table: _table);

            var result = new SellCommand().Execute(state, ctx);

            Assert.That(result.NewState.CurrentLevel, Is.EqualTo(0));
            Assert.That(result.NewState.CurrentSword.Name, Is.EqualTo("나무검"));
        }

        [Test]
        public void Execute_IncrementsTotalSells()
        {
            var state = TestHelper.CreateStateAtLevel(1, gold: 50);
            var ctx = TestHelper.CreateContext(table: _table);

            var result = new SellCommand().Execute(state, ctx);

            Assert.That(result.NewState.PlayerData.Stats.TotalSells, Is.EqualTo(1));
        }

        [Test]
        public void Execute_EmitsSellEvent()
        {
            var state = TestHelper.CreateStateAtLevel(2, gold: 50);
            var ctx = TestHelper.CreateContext(table: _table);

            var result = new SellCommand().Execute(state, ctx);

            var evt = result.Events.OfType<SellEvent>().Single();
            Assert.That(evt.SoldLevel, Is.EqualTo(2));
            Assert.That(evt.SoldSwordName, Is.EqualTo("강철검"));
            Assert.That(evt.GoldGained, Is.EqualTo(10));
        }

        [Test]
        public void Execute_EmitsGoldChangeEvent()
        {
            var state = TestHelper.CreateStateAtLevel(1, gold: 50);
            var ctx = TestHelper.CreateContext(table: _table);

            var result = new SellCommand().Execute(state, ctx);

            var goldEvt = result.Events.OfType<GoldChangeEvent>().Single();
            Assert.That(goldEvt.Amount, Is.EqualTo(4));
            Assert.That(goldEvt.NewTotal, Is.EqualTo(54));
            Assert.That(goldEvt.Reason, Is.EqualTo("sell"));
        }

        [Test]
        public void Execute_UpdatesTotalGoldEarned()
        {
            var state = TestHelper.CreateStateAtLevel(3, gold: 50); // sell=19
            var ctx = TestHelper.CreateContext(table: _table);

            var result = new SellCommand().Execute(state, ctx);

            Assert.That(result.NewState.PlayerData.Stats.TotalGoldEarned, Is.EqualTo(19));
        }
    }
}
