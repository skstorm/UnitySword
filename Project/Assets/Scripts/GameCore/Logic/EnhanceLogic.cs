using System;
using System.Collections.Generic;
using GameCore.Commands;
using GameCore.Data;
using GameCore.Events;
using GameCore.Models;
using GameCore.Util;

namespace GameCore.Logic
{
    internal class EnhanceLogic
    {
        public double GetEffectiveRate(GameState state, Sword targetSword)
        {
            return Math.Clamp(targetSword.SuccessRate, 0.0, 1.0);
        }

        public bool Roll(double effectiveRate, IRandomProvider random)
        {
            return random.NextDouble() < effectiveRate;
        }

        public LogicResult HandleSuccess(GameState state, Sword newSword, SwordDataTable table,
            int discountedNextCost = -1)
        {
            var newLevel = state.CurrentLevel + 1;
            var newState = state.With(
                currentSword: newSword,
                currentLevel: newLevel);

            // Update statistics
            var stats = newState.PlayerData.Stats;
            var newConsecutiveSuccess = stats.CurrentConsecutiveSuccess + 1;
            var newStats = stats.With(
                totalEnhanceAttempts: stats.TotalEnhanceAttempts + 1,
                highestEnhanceLevel: Math.Max(stats.HighestEnhanceLevel, newLevel),
                weeklyHighestLevel: Math.Max(stats.WeeklyHighestLevel, newLevel),
                maxConsecutiveSuccess: Math.Max(stats.MaxConsecutiveSuccess, newConsecutiveSuccess),
                currentConsecutiveSuccess: newConsecutiveSuccess,
                currentConsecutiveFail: 0);
            newState = newState.With(playerData: newState.PlayerData.With(stats: newStats));

            var nextSword = table.GetSword(newLevel + 1);
            var nextCost = discountedNextCost >= 0 ? discountedNextCost : (nextSword?.EnhanceCost ?? 0);

            return new LogicResult(newState, new List<GameEvent>
            {
                new EnhanceSuccessEvent(state.CurrentLevel, newLevel, newSword.Name, nextCost)
            });
        }

        public LogicResult HandleFail(GameState state, SwordDataTable table, GameContext context)
        {
            var events = new List<GameEvent>();
            var stats = state.PlayerData.Stats;
            var newConsecutiveFail = stats.CurrentConsecutiveFail + 1;

            if (state.HasActiveProtection)
            {
                // Protection amulet absorbs the destruction — no destroy count
                var protectedStats = stats.With(
                    totalEnhanceAttempts: stats.TotalEnhanceAttempts + 1,
                    maxConsecutiveFail: Math.Max(stats.MaxConsecutiveFail, newConsecutiveFail),
                    currentConsecutiveFail: newConsecutiveFail,
                    currentConsecutiveSuccess: 0);
                var protectedState = state.With(
                    hasActiveProtection: false,
                    playerData: state.PlayerData.With(stats: protectedStats));
                events.Add(new EnhanceFailEvent(
                    state.CurrentLevel, state.CurrentSword.Name,
                    0, 0, false, destroyed: false));
                return new LogicResult(protectedState, events);
            }

            // Actual destruction
            var newStats = stats.With(
                totalEnhanceAttempts: stats.TotalEnhanceAttempts + 1,
                totalDestroys: stats.TotalDestroys + 1,
                maxConsecutiveFail: Math.Max(stats.MaxConsecutiveFail, newConsecutiveFail),
                currentConsecutiveFail: newConsecutiveFail,
                currentConsecutiveSuccess: 0);
            var newState = state.With(playerData: state.PlayerData.With(stats: newStats));

            var adAvailable = state.PlayerData.AdLimits.AdProtectionUsedToday < 2;
            newState = newState.With(pendingAdProtection: adAvailable);

            events.Add(new EnhanceFailEvent(
                state.CurrentLevel, state.CurrentSword.Name,
                0, 0, adAvailable, destroyed: true));

            return new LogicResult(newState, events);
        }
    }
}
