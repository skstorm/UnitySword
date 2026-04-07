using NUnit.Framework;
using GameCore.Commands;
using GameCore.Events;
using GameCore.Models;
using System.Linq;

namespace GameCore.Tests.Commands
{
    [TestFixture]
    public class ConfirmDestroyCommandTests
    {
        private GameCore.Data.SwordDataTable _table;

        [SetUp]
        public void SetUp()
        {
            _table = TestHelper.CreateSmallTable();
        }

        // --- Validate ---

        [Test]
        public void Validate_RejectsWhenNoPendingDestruction()
        {
            var state = TestHelper.CreateStateAtLevel(1);
            var ctx = TestHelper.CreateContext(table: _table);

            Assert.That(new ConfirmDestroyCommand().Validate(state, ctx),
                Is.EqualTo("no_pending_destruction"));
        }

        [Test]
        public void Validate_AcceptsWhenPending()
        {
            var state = TestHelper.CreateStateAtLevel(1)
                .With(pendingAdProtection: true);
            var ctx = TestHelper.CreateContext(table: _table);

            Assert.That(new ConfirmDestroyCommand().Validate(state, ctx), Is.Null);
        }

        // --- Execute ---

        [Test]
        public void Execute_ResetsToWoodenSword()
        {
            var state = TestHelper.CreateStateAtLevel(3)
                .With(pendingAdProtection: true);
            var ctx = TestHelper.CreateContext(table: _table);

            var result = new ConfirmDestroyCommand().Execute(state, ctx);

            Assert.That(result.NewState.CurrentLevel, Is.EqualTo(0));
            Assert.That(result.NewState.CurrentSword.Name, Is.EqualTo("나무검"));
            Assert.That(result.NewState.PendingAdProtection, Is.False);
        }

        [Test]
        public void Execute_EmitsDestroyConfirmedEvent()
        {
            var state = TestHelper.CreateStateAtLevel(4)
                .With(pendingAdProtection: true);
            var ctx = TestHelper.CreateContext(table: _table);

            var result = new ConfirmDestroyCommand().Execute(state, ctx);

            var evt = result.Events.OfType<DestroyConfirmedEvent>().Single();
            Assert.That(evt.DestroyedLevel, Is.EqualTo(4));
            Assert.That(evt.DestroyedSwordName, Is.EqualTo("백은검"));
        }

        [Test]
        public void Execute_PreservesGold()
        {
            var state = TestHelper.CreateStateAtLevel(2, gold: 777)
                .With(pendingAdProtection: true);
            var ctx = TestHelper.CreateContext(table: _table);

            var result = new ConfirmDestroyCommand().Execute(state, ctx);

            Assert.That(result.NewState.PlayerData.Gold, Is.EqualTo(777));
        }
    }
}
