using System.Collections.Generic;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands
{
    public class CollectCommand : Command
    {
        public override string Validate(GameState state, GameContext context)
        {
            if (state.PendingAdProtection) return "pending_ad_protection";
            if (!state.CurrentSword.Collectible) return "not_collectible";
            return null;
        }

        public override CommandResult Execute(GameState state, GameContext context)
        {
            var collectResult = new CollectionLogic().Collect(state);
            var s = collectResult.NewState;
            var events = new List<GameEvent>(collectResult.Events);

            // Reset to wooden sword after collecting
            s = new GameSessionLogic().ResetToWoodenSword(s, context.SwordTable);

            return new CommandResult(s, events);
        }
    }
}
