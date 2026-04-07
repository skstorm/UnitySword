using System.Collections.Generic;
using GameCore.Commands;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Logic
{
    internal class EconomyLogic
    {
        public bool CanAfford(GameState state, int cost)
        {
            return state.PlayerData.Gold >= cost;
        }

        public LogicResult SpendGold(GameState state, int cost)
        {
            var newGold = state.PlayerData.Gold - cost;
            var newState = state.With(playerData: state.PlayerData.With(gold: newGold));
            return new LogicResult(newState, new List<GameEvent>
            {
                new GoldChangeEvent(-cost, newGold, "enhance")
            });
        }

        public LogicResult AddGold(GameState state, int amount, string reason)
        {
            var newGold = state.PlayerData.Gold + amount;
            var newStats = state.PlayerData.Stats.With(
                totalGoldEarned: state.PlayerData.Stats.TotalGoldEarned + amount);
            var newState = state.With(playerData: state.PlayerData.With(gold: newGold, stats: newStats));
            return new LogicResult(newState, new List<GameEvent>
            {
                new GoldChangeEvent(amount, newGold, reason)
            });
        }
    }
}
