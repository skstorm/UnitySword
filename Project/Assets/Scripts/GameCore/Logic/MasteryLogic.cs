using System.Collections.Generic;
using GameCore.Commands;
using GameCore.Data;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Logic
{
    internal class MasteryLogic
    {
        public LogicResult AddExp(GameState state, MasteryDataTable masteryTable)
        {
            var events = new List<GameEvent>();
            var mastery = state.PlayerData.Mastery;
            var newAttempts = mastery.TotalAttempts + 1;

            if (masteryTable == null)
            {
                var newMasteryNoTable = mastery.With(totalAttempts: newAttempts);
                var newStateNoTable = state.With(playerData: state.PlayerData.With(mastery: newMasteryNoTable));
                events.Add(new MasteryExpEvent(newAttempts, mastery.Level));
                return new LogicResult(newStateNoTable, events);
            }

            var currentLevel = mastery.Level;

            // Check for level ups (may skip multiple levels if exp is high enough)
            var newLevel = currentLevel;
            while (newLevel < masteryTable.MaxLevel)
            {
                var nextLevelData = masteryTable.GetLevel(newLevel + 1);
                if (nextLevelData == null || newAttempts < nextLevelData.RequiredExp)
                    break;
                newLevel++;
            }

            var newMastery = mastery.With(level: newLevel, totalAttempts: newAttempts);
            var newState = state.With(playerData: state.PlayerData.With(mastery: newMastery));

            events.Add(new MasteryExpEvent(newAttempts, newLevel));

            if (newLevel > currentLevel)
            {
                var levelData = masteryTable.GetLevel(newLevel);
                events.Add(new MasteryLevelUpEvent(currentLevel, newLevel,
                    levelData?.RewardDescription ?? ""));
            }

            return new LogicResult(newState, events);
        }

        public double GetCostDiscount(GameState state, MasteryDataTable masteryTable)
        {
            if (masteryTable == null) return 0;
            var levelData = masteryTable.GetLevel(state.PlayerData.Mastery.Level);
            return levelData?.CostDiscount ?? 0;
        }

        public int GetFragmentBonus(GameState state, MasteryDataTable masteryTable)
        {
            if (masteryTable == null) return 0;
            var levelData = masteryTable.GetLevel(state.PlayerData.Mastery.Level);
            return levelData?.FragmentBonus ?? 0;
        }
    }
}
