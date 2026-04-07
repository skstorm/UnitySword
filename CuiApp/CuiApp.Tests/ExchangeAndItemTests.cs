using GameCore.Commands;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;
using NUnit.Framework;

namespace CuiApp.Tests;

/// <summary>CT-3: Exchange and item usage tests.</summary>
[TestFixture]
public class ExchangeAndItemTests
{
    [Test]
    public void Exchange_InsufficientFragments_Rejected()
    {
        var (engine, _) = TestSetup.CreateGame();
        var rejected = false;
        engine.OnEvent += e => { if (e is CommandRejectedEvent) rejected = true; };

        engine.Dispatch(new ExchangeCommand(ExchangeItemType.ProtectionAmulet));

        Assert.That(rejected, Is.True);
    }

    [Test]
    public void Exchange_ProtectionAmulet_NotEnoughFragments_Rejected()
    {
        // 3 fragments — not enough for amulet (10)
        var context = TestSetup.CreateContext();
        var pd = PlayerData.CreateDefault().With(gold: 200, fragments: 3);
        var state = new GameState(context.SwordTable.GetSword(0)!, 0, pd);
        var engine = new GameEngine(state, context);

        var rejected = false;
        engine.OnEvent += e => { if (e is CommandRejectedEvent) rejected = true; };
        engine.Dispatch(new ExchangeCommand(ExchangeItemType.ProtectionAmulet));
        Assert.That(rejected, Is.True);
    }

    [Test]
    public void Exchange_GoldPouch_Costs5Fragments_Gives500G()
    {
        var context = TestSetup.CreateContext();
        var pd = PlayerData.CreateDefault().With(gold: 200, fragments: 10);
        var state = new GameState(context.SwordTable.GetSword(0)!, 0, pd);
        var engine = new GameEngine(state, context);

        engine.Dispatch(new ExchangeCommand(ExchangeItemType.GoldPouch));

        Assert.That(engine.State.PlayerData.Gold, Is.EqualTo(700)); // 200 + 500
        Assert.That(engine.State.PlayerData.Fragments, Is.EqualTo(5)); // 10 - 5
    }

    [Test]
    public void UseItem_ProtectionAmulet_WithoutOwning_Rejected()
    {
        var (engine, _) = TestSetup.CreateGame();
        var rejected = false;
        engine.OnEvent += e => { if (e is CommandRejectedEvent) rejected = true; };

        engine.Dispatch(new UseItemCommand(UseItemType.ProtectionAmulet));

        Assert.That(rejected, Is.True);
    }

    [Test]
    public void UseItem_BlessingScroll_AddsModifier()
    {
        // Manually create state with a blessing scroll
        var context = TestSetup.CreateContext(0.01);
        var pd = PlayerData.CreateDefault().With(
            gold: 200,
            items: new Inventory(blessingScrolls: 1));
        var state = new GameState(
            context.SwordTable.GetSword(0)!, 0, pd);
        var engine = new GameEngine(state, context);

        engine.Dispatch(new UseItemCommand(UseItemType.BlessingScroll));

        Assert.That(engine.State.ActiveModifiers.Count, Is.EqualTo(1));
        Assert.That(engine.State.PlayerData.Items.BlessingScrolls, Is.EqualTo(0));
    }

    [Test]
    public void UseItem_ProtectionAmulet_ActivatesProtection()
    {
        var context = TestSetup.CreateContext();
        var pd = PlayerData.CreateDefault().With(
            gold: 200,
            items: new Inventory(protectionAmulets: 1));
        var state = new GameState(
            context.SwordTable.GetSword(0)!, 0, pd);
        var engine = new GameEngine(state, context);

        engine.Dispatch(new UseItemCommand(UseItemType.ProtectionAmulet));

        Assert.That(engine.State.HasActiveProtection, Is.True);
        Assert.That(engine.State.PlayerData.Items.ProtectionAmulets, Is.EqualTo(0));
    }

    [Test]
    public void Protection_PreventDestruction_OnFail()
    {
        // State: +1 sword with protection active, then fail
        var context = TestSetup.CreateContext(0.99); // fail
        var pd = PlayerData.CreateDefault().With(gold: 200);
        var sword = context.SwordTable.GetSword(1)!;
        var state = new GameState(sword, 1, pd, hasActiveProtection: true);
        var engine = new GameEngine(state, context);

        engine.Dispatch(new EnhanceCommand());

        // Should still be +1 (not destroyed)
        Assert.That(engine.State.CurrentLevel, Is.EqualTo(1));
        Assert.That(engine.State.HasActiveProtection, Is.False);
    }
}
