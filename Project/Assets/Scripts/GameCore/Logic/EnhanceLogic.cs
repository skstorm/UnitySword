using System;
using System.Collections.Generic;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;
using GameCore.Util;

namespace GameCore.Logic;

internal class EnhanceLogic
{
    public double GetEffectiveRate(GameState state, Sword targetSword)
    {
        double rate = targetSword.SuccessRate;
        foreach (var mod in state.ActiveModifiers)
        {
            rate = mod.Apply(rate);
        }
        return Math.Clamp(rate, 0.0, 1.0);
    }

    public bool Roll(double effectiveRate, IRandomProvider random)
    {
        return random.NextDouble() < effectiveRate;
    }

    public LogicResult HandleSuccess(GameState state, Sword newSword, SwordDataTable table)
    {
        var newLevel = state.CurrentLevel + 1;
        var newState = state.With(
            currentSword: newSword,
            currentLevel: newLevel,
            activeModifiers: new List<IModifier>());

        var nextSword = table.GetSword(newLevel + 1);
        var nextCost = nextSword?.EnhanceCost ?? 0;

        var events = new List<GameEvent>
        {
            new EnhanceSuccessEvent(state.CurrentLevel, newLevel, newSword.Name, nextCost)
        };

        return new LogicResult(newState, events);
    }

    public LogicResult HandleFail(GameState state, SwordDataTable table, GameContext context)
    {
        if (state.HasActiveProtection)
        {
            var ns = state.With(
                hasActiveProtection: false,
                activeModifiers: new List<IModifier>());

            var events = new List<GameEvent>
            {
                new EnhanceFailEvent(
                    state.CurrentLevel,
                    state.CurrentSword.Name,
                    state.CurrentSword.SellPrice,
                    0,
                    false,
                    false)
            };

            return new LogicResult(ns, events);
        }

        var adAvailable = context.IsAdSystemEnabled && state.PlayerData.AdLimits.CanUseAdProtection();
        var newState = state.With(
            pendingAdProtection: adAvailable,
            activeModifiers: new List<IModifier>());

        var failEvents = new List<GameEvent>
        {
            new EnhanceFailEvent(
                state.CurrentLevel,
                state.CurrentSword.Name,
                state.CurrentSword.SellPrice,
                state.CurrentSword.FragmentReward,
                adAvailable,
                true)
        };

        return new LogicResult(newState, failEvents);
    }
}
