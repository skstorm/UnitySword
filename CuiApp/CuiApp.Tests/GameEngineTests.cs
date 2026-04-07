using GameCore.Commands;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;
using NUnit.Framework;

namespace CuiApp.Tests;

/// <summary>CT-2: Smoke test — initialization, enhance, sell, collect flow.</summary>
[TestFixture]
public class GameEngineTests
{
    [Test]
    public void InitialState_HasWoodenSword_And200Gold()
    {
        var (engine, _) = TestSetup.CreateGame();

        Assert.That(engine.State.CurrentLevel, Is.EqualTo(0));
        Assert.That(engine.State.CurrentSword.Name, Is.EqualTo("나무검"));
        Assert.That(engine.State.PlayerData.Gold, Is.EqualTo(200));
    }

    [Test]
    public void Enhance_Success_IncreasesLevel()
    {
        // random = 0.01 → success (rate for +1 is 95%)
        var (engine, context) = TestSetup.CreateGame(0.01);

        engine.Dispatch(new EnhanceCommand());

        Assert.That(engine.State.CurrentLevel, Is.EqualTo(1));
        Assert.That(engine.State.CurrentSword.Name, Is.EqualTo("철검"));
    }

    [Test]
    public void Enhance_Fail_DestroysAndResetsToWooden()
    {
        // random = 0.99 → fail (rate for +1 is 95%)
        var (engine, context) = TestSetup.CreateGame(0.99);

        engine.Dispatch(new EnhanceCommand());

        Assert.That(engine.State.CurrentLevel, Is.EqualTo(0));
        Assert.That(engine.State.CurrentSword.Name, Is.EqualTo("나무검"));
    }

    [Test]
    public void Enhance_EmitsCorrectEvents()
    {
        var (engine, _) = TestSetup.CreateGame(0.01); // success
        var events = new List<GameEvent>();
        engine.OnEvent += e => events.Add(e);

        engine.Dispatch(new EnhanceCommand());

        Assert.That(events.Any(e => e is GoldChangeEvent), Is.True);
        Assert.That(events.Any(e => e is EnhanceSuccessEvent), Is.True);
    }

    [Test]
    public void Enhance_Fail_GivesFragments()
    {
        // Set up: start at +6 with enough gold, then fail +7
        var context = TestSetup.CreateContext(0.99); // fail
        var sword = context.SwordTable.GetSword(6)!;
        var pd = PlayerData.CreateDefault().With(gold: 10000);
        var state = new GameState(sword, 6, pd);
        var engine = new GameEngine(state, context);

        engine.Dispatch(new EnhanceCommand());

        // +6 미스릴검 gives 3 fragments on destroy
        Assert.That(engine.State.CurrentLevel, Is.EqualTo(0));
        Assert.That(engine.State.PlayerData.Fragments, Is.EqualTo(3));
    }

    [Test]
    public void Sell_GivesGoldAndResetsToWooden()
    {
        var (engine, _) = TestSetup.CreateGame(0.01);
        engine.Dispatch(new EnhanceCommand()); // +1 철검

        var goldBefore = engine.State.PlayerData.Gold;
        engine.Dispatch(new SellCommand());

        Assert.That(engine.State.CurrentLevel, Is.EqualTo(0));
        Assert.That(engine.State.PlayerData.Gold, Is.GreaterThan(goldBefore));
    }

    [Test]
    public void Sell_WoodenSword_Rejected()
    {
        var (engine, _) = TestSetup.CreateGame();
        var rejected = false;
        engine.OnEvent += e => { if (e is CommandRejectedEvent) rejected = true; };

        engine.Dispatch(new SellCommand());

        Assert.That(rejected, Is.True);
        Assert.That(engine.State.CurrentLevel, Is.EqualTo(0));
    }

    [Test]
    public void EmergencyFund_OnlyWhenBroke()
    {
        var (engine, _) = TestSetup.CreateGame();

        // Has 200G, can afford +1 (costs 5G) → should be rejected
        var rejected = false;
        engine.OnEvent += e => { if (e is CommandRejectedEvent) rejected = true; };
        engine.Dispatch(new EmergencyFundCommand());

        Assert.That(rejected, Is.True);
    }

    [Test]
    public void Enhance_GrantsMasteryExp()
    {
        var (engine, _) = TestSetup.CreateGame(0.01);

        engine.Dispatch(new EnhanceCommand());

        // Level 0 → masteryExpGain = 1 + 0 = 1
        Assert.That(engine.State.PlayerData.MasteryExp, Is.EqualTo(1));
    }

    [Test]
    public void Statistics_TracksEnhanceCount()
    {
        var (engine, _) = TestSetup.CreateGame(0.01, 0.01);

        engine.Dispatch(new EnhanceCommand());
        engine.Dispatch(new EnhanceCommand());

        Assert.That(engine.State.PlayerData.Stats.TotalEnhances, Is.EqualTo(2));
    }
}
