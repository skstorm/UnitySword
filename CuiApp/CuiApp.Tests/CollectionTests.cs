using System.Collections.Generic;
using GameCore.Commands;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;
using NUnit.Framework;

namespace CuiApp.Tests;

/// <summary>CT-4: Collection and mastery tests.</summary>
[TestFixture]
public class CollectionTests
{
    [Test]
    public void Collect_BelowLevel10_Rejected()
    {
        // State at +5
        var context = TestSetup.CreateContext();
        var pd = PlayerData.CreateDefault().With(gold: 1000);
        var sword = context.SwordTable.GetSword(5)!;
        var state = new GameState(sword, 5, pd);
        var engine = new GameEngine(state, context);

        var rejected = false;
        engine.OnEvent += e => { if (e is CommandRejectedEvent) rejected = true; };
        engine.Dispatch(new CollectCommand());

        Assert.That(rejected, Is.True);
    }

    [Test]
    public void Collect_AtLevel10_AddsToCollection()
    {
        var context = TestSetup.CreateContext();
        var pd = PlayerData.CreateDefault().With(gold: 1000);
        var sword = context.SwordTable.GetSword(10)!;
        var state = new GameState(sword, 10, pd);
        var engine = new GameEngine(state, context);

        engine.Dispatch(new CollectCommand());

        Assert.That(engine.State.PlayerData.CollectedSwords, Contains.Item(10));
        Assert.That(engine.State.CurrentLevel, Is.EqualTo(0)); // reset
    }

    [Test]
    public void Collect_AlreadyCollected_Rejected()
    {
        var context = TestSetup.CreateContext();
        var pd = PlayerData.CreateDefault().With(
            gold: 1000,
            collectedSwords: new List<int> { 10 });
        var sword = context.SwordTable.GetSword(10)!;
        var state = new GameState(sword, 10, pd);
        var engine = new GameEngine(state, context);

        var reason = "";
        engine.OnEvent += e => { if (e is CommandRejectedEvent r) reason = r.Reason; };
        engine.Dispatch(new CollectCommand());

        Assert.That(reason, Is.EqualTo("already_collected"));
    }

    [Test]
    public void Collect_EmitsCollectEvent()
    {
        var context = TestSetup.CreateContext();
        var pd = PlayerData.CreateDefault().With(gold: 1000);
        var sword = context.SwordTable.GetSword(10)!;
        var state = new GameState(sword, 10, pd);
        var engine = new GameEngine(state, context);

        var events = new List<GameEvent>();
        engine.OnEvent += e => events.Add(e);
        engine.Dispatch(new CollectCommand());

        Assert.That(events.Any(e => e is CollectEvent ce && ce.Level == 10), Is.True);
    }

    [Test]
    public void MasteryExp_AccumulatesOverMultipleEnhances()
    {
        // 3 enhances: level 0→1 (exp+1), level 1→2 (exp+2), level 2→3 (exp+3) = 6 total
        var (engine, _) = TestSetup.CreateGame(0.01, 0.01, 0.01);

        engine.Dispatch(new EnhanceCommand()); // at level 0: +1 exp
        engine.Dispatch(new EnhanceCommand()); // at level 1: +2 exp
        engine.Dispatch(new EnhanceCommand()); // at level 2: +3 exp

        Assert.That(engine.State.PlayerData.MasteryExp, Is.EqualTo(6));
    }
}
