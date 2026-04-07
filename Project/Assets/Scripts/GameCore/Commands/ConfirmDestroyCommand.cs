using System.Collections.Generic;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands
{
    public class ConfirmDestroyCommand : Command
    {
        public override string Validate(GameState state, GameContext context)
        {
            if (!state.PendingAdProtection) return "no_pending_destruction";
            return null;
        }

        public override CommandResult Execute(GameState state, GameContext context)
        {
            var destroyedLevel = state.CurrentLevel;
            var destroyedName = state.CurrentSword.Name;

            var result = new DestroyFinalizationLogic().Finalize(
                state, destroyedLevel, destroyedName,
                context.SwordTable, context.MasteryTable);

            return new CommandResult(result.NewState, new List<GameEvent>(result.Events));
        }
    }
}
