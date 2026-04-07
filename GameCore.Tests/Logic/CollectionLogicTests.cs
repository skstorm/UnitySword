using NUnit.Framework;
using GameCore.Events;
using GameCore.Models;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Tests.Logic
{
    [TestFixture]
    public class CollectionLogicTests
    {
        [Test]
        public void Collect_AddsToCollection()
        {
            var fullTable = TestHelper.LoadFullTable();
            var sword = fullTable.GetSword(10);
            var state = new GameState(sword, 10,
                new PlayerData(gold: 0, isFirstRun: false));

            var result = new GameCore.Logic.CollectionLogic().Collect(state);

            Assert.That(result.NewState.PlayerData.Collection.Collected.ContainsKey(10), Is.True);
            Assert.That(result.NewState.PlayerData.Collection.Collected[10], Is.EqualTo(1));
        }

        [Test]
        public void Collect_FirstCollect_FlagIsTrue()
        {
            var fullTable = TestHelper.LoadFullTable();
            var sword = fullTable.GetSword(10);
            var state = new GameState(sword, 10,
                new PlayerData(gold: 0, isFirstRun: false));

            var result = new GameCore.Logic.CollectionLogic().Collect(state);
            var evt = result.Events.OfType<CollectEvent>().Single();

            Assert.That(evt.IsFirstCollect, Is.True);
        }

        [Test]
        public void Collect_DuplicateCollect_IncrementsCount()
        {
            var fullTable = TestHelper.LoadFullTable();
            var sword = fullTable.GetSword(10);
            var collected = new Dictionary<int, int> { { 10, 2 } };
            var collection = new CollectionData(collected);
            var state = new GameState(sword, 10,
                new PlayerData(gold: 0, isFirstRun: false, collection: collection));

            var result = new GameCore.Logic.CollectionLogic().Collect(state);

            Assert.That(result.NewState.PlayerData.Collection.Collected[10], Is.EqualTo(3));
            var evt = result.Events.OfType<CollectEvent>().Single();
            Assert.That(evt.IsFirstCollect, Is.False);
        }

        [Test]
        public void Collect_UniqueCount_Increases()
        {
            var fullTable = TestHelper.LoadFullTable();
            var sword = fullTable.GetSword(12);
            var collected = new Dictionary<int, int> { { 10, 1 }, { 11, 1 } };
            var collection = new CollectionData(collected);
            var state = new GameState(sword, 12,
                new PlayerData(gold: 0, isFirstRun: false, collection: collection));

            var result = new GameCore.Logic.CollectionLogic().Collect(state);

            Assert.That(result.NewState.PlayerData.Collection.UniqueCount, Is.EqualTo(3));
        }

        [Test]
        public void Collect_AllCollected_EmitsCompleteEvent()
        {
            var fullTable = TestHelper.LoadFullTable();
            var sword = fullTable.GetSword(20);
            // Already have 10 unique swords (10~19), collecting last one (20)
            var collected = new Dictionary<int, int>();
            for (int i = 10; i <= 19; i++)
                collected[i] = 1;
            var collection = new CollectionData(collected);
            var state = new GameState(sword, 20,
                new PlayerData(gold: 0, isFirstRun: false, collection: collection));

            var result = new GameCore.Logic.CollectionLogic().Collect(state);

            Assert.That(result.Events.OfType<CollectionCompleteEvent>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Collect_NotAllCollected_NoCompleteEvent()
        {
            var fullTable = TestHelper.LoadFullTable();
            var sword = fullTable.GetSword(10);
            var state = new GameState(sword, 10,
                new PlayerData(gold: 0, isFirstRun: false));

            var result = new GameCore.Logic.CollectionLogic().Collect(state);

            Assert.That(result.Events.OfType<CollectionCompleteEvent>().Count(), Is.EqualTo(0));
        }

        [Test]
        public void CompletionRate_CalculatesCorrectly()
        {
            var collected = new Dictionary<int, int> { { 10, 1 }, { 15, 2 } };
            var collection = new CollectionData(collected);

            // 2 unique out of 11 total
            Assert.That(collection.CompletionRate, Is.EqualTo(2.0 / 11).Within(0.001));
        }
    }
}
