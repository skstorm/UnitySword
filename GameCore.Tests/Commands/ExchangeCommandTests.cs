using NUnit.Framework;
using GameCore.Commands;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;
using GameCore.Util;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Tests.Commands
{
    [TestFixture]
    public class ExchangeCommandTests
    {
        private GameCore.Data.SwordDataTable _table;

        [SetUp]
        public void SetUp()
        {
            _table = TestHelper.CreateSmallTable();
        }

        [Test]
        public void Validate_InsufficientFragments_Amulet()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(0);
            state = state.With(playerData: state.PlayerData.With(fragments: 29));

            var result = new ExchangeCommand("protection_amulet").Validate(state, ctx);
            Assert.That(result, Is.EqualTo("insufficient_fragments"));
        }

        [Test]
        public void Validate_InsufficientFragments_GoldPouch()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(0);
            state = state.With(playerData: state.PlayerData.With(fragments: 9));

            var result = new ExchangeCommand("gold_pouch").Validate(state, ctx);
            Assert.That(result, Is.EqualTo("insufficient_fragments"));
        }

        [Test]
        public void Validate_InvalidItemType()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(0);
            state = state.With(playerData: state.PlayerData.With(fragments: 100));

            var result = new ExchangeCommand("invalid").Validate(state, ctx);
            Assert.That(result, Is.EqualTo("invalid_item_type"));
        }

        [Test]
        public void Validate_PendingAdProtection()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(1);
            state = state.With(pendingAdProtection: true,
                playerData: state.PlayerData.With(fragments: 100));

            var result = new ExchangeCommand("protection_amulet").Validate(state, ctx);
            Assert.That(result, Is.EqualTo("pending_ad_protection"));
        }

        [Test]
        public void Validate_Sufficient_ReturnsNull()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(0);
            state = state.With(playerData: state.PlayerData.With(fragments: 30));

            var result = new ExchangeCommand("protection_amulet").Validate(state, ctx);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Execute_ProtectionAmulet_DeductsFragments()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(0);
            state = state.With(playerData: state.PlayerData.With(fragments: 50));

            var result = new ExchangeCommand("protection_amulet").Execute(state, ctx);

            Assert.That(result.NewState.PlayerData.Fragments, Is.EqualTo(20));
            Assert.That(result.NewState.PlayerData.Inventory.ProtectionAmulets, Is.EqualTo(1));
        }

        [Test]
        public void Execute_ProtectionAmulet_EmitsEvent()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(0);
            state = state.With(playerData: state.PlayerData.With(fragments: 30));

            var result = new ExchangeCommand("protection_amulet").Execute(state, ctx);
            var evt = result.Events.OfType<ExchangeEvent>().Single();

            Assert.That(evt.ItemType, Is.EqualTo("protection_amulet"));
            Assert.That(evt.FragmentCost, Is.EqualTo(30));
            Assert.That(evt.RemainingFragments, Is.EqualTo(0));
        }

        [Test]
        public void Execute_GoldPouch_AddsGold()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(0);
            state = state.With(playerData: state.PlayerData.With(fragments: 10, gold: 100));

            var result = new ExchangeCommand("gold_pouch").Execute(state, ctx);

            Assert.That(result.NewState.PlayerData.Fragments, Is.EqualTo(0));
            Assert.That(result.NewState.PlayerData.Gold, Is.EqualTo(150)); // 100+50
        }

        [Test]
        public void Execute_GoldPouch_EmitsExchangeAndGoldEvents()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(0);
            state = state.With(playerData: state.PlayerData.With(fragments: 10, gold: 0));

            var result = new ExchangeCommand("gold_pouch").Execute(state, ctx);

            Assert.That(result.Events.OfType<ExchangeEvent>().Count(), Is.EqualTo(1));
            Assert.That(result.Events.OfType<GoldChangeEvent>().Count(), Is.EqualTo(1));

            var goldEvt = result.Events.OfType<GoldChangeEvent>().Single();
            Assert.That(goldEvt.Amount, Is.EqualTo(50));
        }
    }
}
