using System.Collections.Generic;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands;

/// <summary>현재 검을 판매하고 골드를 획득한 뒤 나무검으로 리셋하는 커맨드.</summary>
public class SellCommand : Command
{
    /// <summary>나무검은 판매 불가. 광고 보호 대기 중에도 불가.</summary>
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
        var sellPrice = state.CurrentSword.SellPrice;

        // 판매 대금 지급
        var addResult = context.EconomyLogic.AddGold(state, sellPrice, "sell_sword");
        var currentState = addResult.NewState;
        var allEvents = new List<GameEvent>(addResult.Events);

        allEvents.Add(new SellEvent(
            state.CurrentSword.Name,
            state.CurrentLevel,
            sellPrice));

        // 나무검으로 리셋
        currentState = GameSessionLogic.ResetToWoodenSword(currentState, context.SwordTable);

        return new CommandResult(currentState, allEvents);
    }
}
