using NUnit.Framework;
using GameCore.Commands;
using GameCore.Events;
using GameCore.Models;
using System.Linq;

namespace GameCore.Tests.Commands
{
    [TestFixture]
    public class UseItemCommandTests
    {
        private GameCore.Data.SwordDataTable _table;

        [SetUp]
        public void SetUp()
        {
            _table = TestHelper.CreateSmallTable();
        }

        [Test]
        public void Validate_NoAmulets_Rejected()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(1);

            var result = new UseItemCommand().Validate(state, ctx);
            Assert.That(result, Is.EqualTo("no_amulets"));
        }

        [Test]
        public void Validate_AlreadyProtected_Rejected()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(1);
            state = state.With(
                hasActiveProtection: true,
                playerData: state.PlayerData.With(
                    inventory: new Inventory(protectionAmulets: 1)));

            var result = new UseItemCommand().Validate(state, ctx);
            Assert.That(result, Is.EqualTo("already_protected"));
        }

        [Test]
        public void Validate_WoodenSword_Rejected()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(0);
            state = state.With(playerData: state.PlayerData.With(
                inventory: new Inventory(protectionAmulets: 1)));

            var result = new UseItemCommand().Validate(state, ctx);
            Assert.That(result, Is.EqualTo("cannot_protect_wooden_sword"));
        }

        [Test]
        public void Validate_PendingAdProtection_Rejected()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(1);
            state = state.With(
                pendingAdProtection: true,
                playerData: state.PlayerData.With(
                    inventory: new Inventory(protectionAmulets: 1)));

            var result = new UseItemCommand().Validate(state, ctx);
            Assert.That(result, Is.EqualTo("pending_ad_protection"));
        }

        [Test]
        public void Validate_Valid_ReturnsNull()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(1);
            state = state.With(playerData: state.PlayerData.With(
                inventory: new Inventory(protectionAmulets: 1)));

            var result = new UseItemCommand().Validate(state, ctx);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Execute_ActivatesProtection()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(1);
            state = state.With(playerData: state.PlayerData.With(
                inventory: new Inventory(protectionAmulets: 2)));

            var result = new UseItemCommand().Execute(state, ctx);

            Assert.That(result.NewState.HasActiveProtection, Is.True);
            Assert.That(result.NewState.PlayerData.Inventory.ProtectionAmulets, Is.EqualTo(1));
        }

        [Test]
        public void Execute_EmitsEvent()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var state = TestHelper.CreateStateAtLevel(1);
            state = state.With(playerData: state.PlayerData.With(
                inventory: new Inventory(protectionAmulets: 1)));

            var result = new UseItemCommand().Execute(state, ctx);

            var evt = result.Events.OfType<UseItemEvent>().Single();
            Assert.That(evt.ItemType, Is.EqualTo("protection_amulet"));
            Assert.That(evt.RemainingCount, Is.EqualTo(0));
        }
    }
}
