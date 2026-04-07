using System.Collections.Generic;
using GameCore.Commands;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Logic;

/// <summary>골드 수입/지출 처리를 담당하는 로직 클래스.</summary>
internal class EconomyLogic
{
    /// <summary>현재 골드로 비용을 감당할 수 있는지 확인한다.</summary>
    public bool CanAfford(GameState state, int cost)
    {
        return state.PlayerData.Gold >= cost;
    }

    /// <summary>골드를 차감하고 GoldChangeEvent를 발행한다.</summary>
    public CommandResult SpendGold(GameState state, int cost)
    {
        var newGold = state.PlayerData.Gold - cost;
        var newPlayerData = state.PlayerData.With(gold: newGold);
        var newState = state.With(playerData: newPlayerData);

        var events = new List<GameEvent>
        {
            new GoldChangeEvent(-cost, newGold, "enhance_cost")
        };

        return new CommandResult(newState, events);
    }

    /// <summary>골드를 지급하고 GoldChangeEvent를 발행한다.</summary>
    public CommandResult AddGold(GameState state, int amount, string reason)
    {
        var newGold = state.PlayerData.Gold + amount;
        var newPlayerData = state.PlayerData.With(gold: newGold);
        var newState = state.With(playerData: newPlayerData);

        var events = new List<GameEvent>
        {
            new GoldChangeEvent(amount, newGold, reason)
        };

        return new CommandResult(newState, events);
    }
}
