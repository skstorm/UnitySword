using System.Collections.Generic;
using GameCore.Events;
using GameCore.Models;
using GameCore.Util;

namespace GameCore.Logic;

internal class EconomyLogic
{
    public bool CanAfford(GameState state, int cost)
    {
        return state.PlayerData.Gold >= cost;
    }

    public LogicResult SpendGold(GameState state, int cost)
    {
        var newGold = state.PlayerData.Gold - cost;
        var newPlayerData = state.PlayerData.With(gold: newGold);
        var newState = state.With(playerData: newPlayerData);

        var events = new List<GameEvent>
        {
            new GoldChangeEvent(-cost, newGold, "enhance_cost")
        };

        return new LogicResult(newState, events);
    }

    public LogicResult AddGold(GameState state, int amount, string reason)
    {
        var newGold = state.PlayerData.Gold + amount;
        var newPlayerData = state.PlayerData.With(gold: newGold);
        var newState = state.With(playerData: newPlayerData);

        var events = new List<GameEvent>
        {
            new GoldChangeEvent(amount, newGold, reason)
        };

        return new LogicResult(newState, events);
    }
}
