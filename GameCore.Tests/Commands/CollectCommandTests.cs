using NUnit.Framework;
using GameCore.Commands;
using GameCore.Events;
using GameCore.Models;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Tests.Commands
{
    [TestFixture]
    public class CollectCommandTests
    {
        [Test]
        public void Validate_NotCollectible_Rejected()
        {
            var table = TestHelper.CreateSmallTable();
            var ctx = TestHelper.CreateContext(table: table);
            // Small table swords are all non-collectible
            var state = TestHelper.CreateStateAtLevel(3);

            var result = new CollectCommand().Validate(state, ctx);
            Assert.That(result, Is.EqualTo("not_collectible"));
        }

        [Test]
        public void Validate_PendingAdProtection_Rejected()
        {
            var fullTable = TestHelper.LoadFullTable();
            var ctx = new GameContext(
                new GameCore.Util.FakeRandomProvider(),
                new GameCore.Util.FakeTimeProvider(),
                fullTable);
            var sword = fullTable.GetSword(10);
            var state = new GameState(sword, 10,
                new PlayerData(gold: 0, isFirstRun: false));
            state = state.With(pendingAdProtection: true);

            var result = new CollectCommand().Validate(state, ctx);
            Assert.That(result, Is.EqualTo("pending_ad_protection"));
        }

        [Test]
        public void Validate_Collectible_ReturnsNull()
        {
            var fullTable = TestHelper.LoadFullTable();
            var ctx = new GameContext(
                new GameCore.Util.FakeRandomProvider(),
                new GameCore.Util.FakeTimeProvider(),
                fullTable);
            var sword = fullTable.GetSword(10);
            var state = new GameState(sword, 10,
                new PlayerData(gold: 0, isFirstRun: false));

            var result = new CollectCommand().Validate(state, ctx);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Execute_CollectsAndResetsToWoodenSword()
        {
            var fullTable = TestHelper.LoadFullTable();
            var ctx = new GameContext(
                new GameCore.Util.FakeRandomProvider(),
                new GameCore.Util.FakeTimeProvider(),
                fullTable);
            var sword = fullTable.GetSword(15);
            var state = new GameState(sword, 15,
                new PlayerData(gold: 500, isFirstRun: false));

            var result = new CollectCommand().Execute(state, ctx);

            // Should be collected
            Assert.That(result.NewState.PlayerData.Collection.Collected.ContainsKey(15), Is.True);
            // Should reset to wooden sword
            Assert.That(result.NewState.CurrentLevel, Is.EqualTo(0));
            Assert.That(result.NewState.CurrentSword.Name, Is.EqualTo("나무검"));
            // Gold preserved
            Assert.That(result.NewState.PlayerData.Gold, Is.EqualTo(500));
        }

        [Test]
        public void Execute_EmitsCollectEvent()
        {
            var fullTable = TestHelper.LoadFullTable();
            var ctx = new GameContext(
                new GameCore.Util.FakeRandomProvider(),
                new GameCore.Util.FakeTimeProvider(),
                fullTable);
            var sword = fullTable.GetSword(10);
            var state = new GameState(sword, 10,
                new PlayerData(gold: 0, isFirstRun: false));

            var result = new CollectCommand().Execute(state, ctx);

            var evt = result.Events.OfType<CollectEvent>().Single();
            Assert.That(evt.SwordLevel, Is.EqualTo(10));
            Assert.That(evt.IsFirstCollect, Is.True);
        }
    }
}
