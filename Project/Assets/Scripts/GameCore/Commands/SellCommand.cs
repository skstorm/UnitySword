using System.Collections.Generic;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands;

public class SellCommand : Command
{
    public override string Validate(GameState state, GameContext context)
    {
        if (state.CurrentLevel == 0)
            return "cannot_sell_wooden_sword";

        if (state.PendingAdProtection)
            return "pending_ad_protection";

        return null;
    }

    public override CommandResult Execute(GameState state, GameContext context)
    {
        var economyLogic = new EconomyLogic();
        var sellPrice = state.CurrentSword.SellPrice;

        var addResult = economyLogic.AddGold(state, sellPrice, "sell_sword");
        var currentState = addResult.NewState;
        var allEvents = new List<GameEvent>(addResult.Events);

        allEvents.Add(new SellEvent(
            state.CurrentSword.Name,
            state.CurrentLevel,
            sellPrice));

        currentState = GameSessionLogic.ResetToWoodenSword(currentState, context.SwordTable);

        return new CommandResult(currentState, allEvents);
    }
}
