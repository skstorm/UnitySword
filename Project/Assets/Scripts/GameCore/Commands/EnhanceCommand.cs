using System;
using System.Collections.Generic;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands;

public class EnhanceCommand : Command
{
    public override string Validate(GameState state, GameContext context)
    {
        if (state.PendingAdProtection)
            return "pending_ad_protection";

        var nextLevel = state.CurrentLevel + 1;
        var targetSword = context.SwordTable.GetSword(nextLevel);
        if (targetSword == null)
            return "max_level_reached";

        var cost = targetSword.EnhanceCost;
        if (state.PlayerData.Gold < cost)
            return "insufficient_gold";

        return null;
    }

    public override CommandResult Execute(GameState state, GameContext context)
    {
        var enhanceLogic = new EnhanceLogic();
        var economyLogic = new EconomyLogic();

        var nextLevel = state.CurrentLevel + 1;
        var targetSword = context.SwordTable.GetSword(nextLevel);
        var cost = targetSword.EnhanceCost;

        // Spend gold
        var spendResult = economyLogic.SpendGold(state, cost);
        var currentState = spendResult.NewState;
        var allEvents = new List<GameEvent>(spendResult.Events);

        // Roll for success
        var effectiveRate = enhanceLogic.GetEffectiveRate(currentState, targetSword);
        var success = enhanceLogic.Roll(effectiveRate, context.Random);

        // Update statistics
        var stats = currentState.PlayerData.Stats;
        var newTotalEnhances = stats.TotalEnhances + 1;

        if (success)
        {
            var successResult = enhanceLogic.HandleSuccess(currentState, targetSword, context.SwordTable);
            currentState = successResult.NewState;
            allEvents.AddRange(successResult.Events);

            var newConsecutiveSuccess = stats.CurrentConsecutiveSuccess + 1;
            var newMaxConsecutiveSuccess = Math.Max(stats.MaxConsecutiveSuccess, newConsecutiveSuccess);
            var newHighest = Math.Max(stats.HighestEnhance, nextLevel);

            var newStats = stats.With(
                totalEnhances: newTotalEnhances,
                highestEnhance: newHighest,
                currentConsecutiveSuccess: newConsecutiveSuccess,
                maxConsecutiveSuccess: newMaxConsecutiveSuccess,
                currentConsecutiveFail: 0);

            currentState = currentState.With(playerData: currentState.PlayerData.With(stats: newStats));
        }
        else
        {
            var failResult = enhanceLogic.HandleFail(currentState, context.SwordTable, context);
            currentState = failResult.NewState;
            allEvents.AddRange(failResult.Events);

            var newConsecutiveFail = stats.CurrentConsecutiveFail + 1;
            var newMaxConsecutiveFail = Math.Max(stats.MaxConsecutiveFail, newConsecutiveFail);
            var destroyed = !state.HasActiveProtection && !currentState.PendingAdProtection;

            var newStats = stats.With(
                totalEnhances: newTotalEnhances,
                totalDestroys: destroyed ? stats.TotalDestroys + 1 : stats.TotalDestroys,
                currentConsecutiveFail: newConsecutiveFail,
                maxConsecutiveFail: newMaxConsecutiveFail,
                currentConsecutiveSuccess: 0);

            currentState = currentState.With(playerData: currentState.PlayerData.With(stats: newStats));

            // If destroyed (not protected and not pending ad), reset to wooden sword
            if (destroyed)
            {
                var fragments = state.CurrentSword.FragmentReward;
                if (fragments > 0)
                {
                    var newFragments = currentState.PlayerData.Fragments + fragments;
                    currentState = currentState.With(playerData: currentState.PlayerData.With(fragments: newFragments));
                }
                currentState = GameSessionLogic.ResetToWoodenSword(currentState, context.SwordTable);
            }
        }

        return new CommandResult(currentState, allEvents);
    }
}
