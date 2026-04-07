using NUnit.Framework;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Tests.Logic
{
    [TestFixture]
    public class GameSessionLogicTests
    {
        private GameSessionLogic _logic;
        private GameCore.Data.SwordDataTable _table;

        [SetUp]
        public void SetUp()
        {
            _logic = new GameSessionLogic();
            _table = TestHelper.CreateSmallTable();
        }

        // --- CreateInitialState ---

        [Test]
        public void CreateInitialState_FirstRun_Gives200Gold()
        {
            var player = new PlayerData(isFirstRun: true);
            var state = _logic.CreateInitialState(player, _table);

            Assert.That(state.PlayerData.Gold, Is.EqualTo(200));
            Assert.That(state.PlayerData.IsFirstRun, Is.False);
        }

        [Test]
        public void CreateInitialState_NotFirstRun_KeepsExistingGold()
        {
            var player = new PlayerData(gold: 500, isFirstRun: false);
            var state = _logic.CreateInitialState(player, _table);

            Assert.That(state.PlayerData.Gold, Is.EqualTo(500));
        }

        [Test]
        public void CreateInitialState_StartsAtWoodenSword()
        {
            var player = new PlayerData();
            var state = _logic.CreateInitialState(player, _table);

            Assert.That(state.CurrentLevel, Is.EqualTo(0));
            Assert.That(state.CurrentSword.Name, Is.EqualTo("나무검"));
        }

        [Test]
        public void CreateInitialState_NoProtectionFlags()
        {
            var player = new PlayerData();
            var state = _logic.CreateInitialState(player, _table);

            Assert.That(state.HasActiveProtection, Is.False);
            Assert.That(state.PendingAdProtection, Is.False);
        }

        // --- ResetToWoodenSword ---

        [Test]
        public void ResetToWoodenSword_ResetsLevel()
        {
            var state = TestHelper.CreateStateAtLevel(3);
            var result = _logic.ResetToWoodenSword(state, _table);

            Assert.That(result.CurrentLevel, Is.EqualTo(0));
            Assert.That(result.CurrentSword.Name, Is.EqualTo("나무검"));
        }

        [Test]
        public void ResetToWoodenSword_ClearsProtectionFlags()
        {
            var state = TestHelper.CreateStateAtLevel(3)
                .With(hasActiveProtection: true, pendingAdProtection: true);
            var result = _logic.ResetToWoodenSword(state, _table);

            Assert.That(result.HasActiveProtection, Is.False);
            Assert.That(result.PendingAdProtection, Is.False);
        }

        [Test]
        public void ResetToWoodenSword_PreservesGold()
        {
            var state = TestHelper.CreateStateAtLevel(3, gold: 999);
            var result = _logic.ResetToWoodenSword(state, _table);

            Assert.That(result.PlayerData.Gold, Is.EqualTo(999));
        }

        [Test]
        public void ResetToWoodenSword_PreservesStats()
        {
            var state = TestHelper.CreateStateAtLevel(3);
            var stats = state.PlayerData.Stats.With(totalDestroys: 5, totalEnhanceAttempts: 20);
            state = state.With(playerData: state.PlayerData.With(stats: stats));

            var result = _logic.ResetToWoodenSword(state, _table);

            Assert.That(result.PlayerData.Stats.TotalDestroys, Is.EqualTo(5));
            Assert.That(result.PlayerData.Stats.TotalEnhanceAttempts, Is.EqualTo(20));
        }
    }
}
