using NUnit.Framework;
using GameCore.Commands;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;
using GameCore.Util;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Tests.Engine
{
    [TestFixture]
    public class GameEngineTests
    {
        private GameCore.Data.SwordDataTable _table;

        [SetUp]
        public void SetUp()
        {
            _table = TestHelper.CreateSmallTable();
        }

        [Test]
        public void Create_InitializesWithWoodenSword()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var engine = GameEngine.Create(new PlayerData(), ctx);

            Assert.That(engine.State.CurrentLevel, Is.EqualTo(0));
            Assert.That(engine.State.CurrentSword.Name, Is.EqualTo("나무검"));
            Assert.That(engine.State.PlayerData.Gold, Is.EqualTo(200)); // first run bonus
        }

        [Test]
        public void Dispatch_ValidCommand_UpdatesState()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0); // succeed
            var ctx = new GameContext(random, new FakeTimeProvider(), _table);
            var engine = GameEngine.Create(new PlayerData(), ctx);

            engine.Dispatch(new EnhanceCommand());

            Assert.That(engine.State.CurrentLevel, Is.EqualTo(1));
        }

        [Test]
        public void Dispatch_ValidCommand_PublishesEvents()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0);
            var ctx = new GameContext(random, new FakeTimeProvider(), _table);
            var engine = GameEngine.Create(new PlayerData(), ctx);

            var received = new List<GameEvent>();
            engine.OnEvent += e => received.Add(e);

            engine.Dispatch(new EnhanceCommand());

            Assert.That(received.OfType<GoldChangeEvent>().Count(), Is.EqualTo(1));
            Assert.That(received.OfType<EnhanceSuccessEvent>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Dispatch_RejectedCommand_PublishesRejectedEvent()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var engine = GameEngine.Create(new PlayerData(gold: 0, isFirstRun: false), ctx);

            var received = new List<GameEvent>();
            engine.OnEvent += e => received.Add(e);

            engine.Dispatch(new EnhanceCommand()); // no gold

            var rejected = received.OfType<CommandRejectedEvent>().Single();
            Assert.That(rejected.Reason, Is.EqualTo("insufficient_gold"));
            Assert.That(rejected.CommandType, Is.EqualTo("EnhanceCommand"));
        }

        [Test]
        public void Dispatch_RejectedCommand_DoesNotChangeState()
        {
            var ctx = TestHelper.CreateContext(table: _table);
            var engine = GameEngine.Create(new PlayerData(gold: 0, isFirstRun: false), ctx);

            var stateBefore = engine.State;
            engine.Dispatch(new SellCommand()); // level 0, can't sell

            Assert.That(engine.State, Is.SameAs(stateBefore));
        }

        [Test]
        public void Dispatch_EventsArePublishedInOrder()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0);
            var ctx = new GameContext(random, new FakeTimeProvider(), _table);
            var engine = GameEngine.Create(new PlayerData(), ctx);

            var types = new List<string>();
            engine.OnEvent += e => types.Add(e.GetType().Name);

            engine.Dispatch(new EnhanceCommand());

            // GoldChange first, then EnhanceSuccess
            Assert.That(types[0], Is.EqualTo("GoldChangeEvent"));
            Assert.That(types[1], Is.EqualTo("EnhanceSuccessEvent"));
        }

        [Test]
        public void Dispatch_NoSubscribers_DoesNotThrow()
        {
            var random = new FakeRandomProvider();
            random.SetNext(0.0);
            var ctx = new GameContext(random, new FakeTimeProvider(), _table);
            var engine = GameEngine.Create(new PlayerData(), ctx);

            Assert.DoesNotThrow(() => engine.Dispatch(new EnhanceCommand()));
        }
    }
}
