using System.Collections.Generic;
using GameCore.Commands;
using GameCore.Data;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Logic
{
    internal class FragmentLogic
    {
        public LogicResult GiveFragments(GameState state, int destroyedLevel,
            int masteryBonus, SwordDataTable swordTable)
        {
            var sword = swordTable.GetSword(destroyedLevel);
            if (sword == null)
                return new LogicResult(state);

            var baseAmount = sword.FragmentReward;
            var totalGained = baseAmount + masteryBonus;

            if (totalGained <= 0)
                return new LogicResult(state);

            var newFragments = state.PlayerData.Fragments + totalGained;
            var newState = state.With(playerData: state.PlayerData.With(fragments: newFragments));

            return new LogicResult(newState, new List<GameEvent>
            {
                new FragmentGainEvent(baseAmount, masteryBonus, totalGained,
                    newFragments, destroyedLevel)
            });
        }
    }
}
