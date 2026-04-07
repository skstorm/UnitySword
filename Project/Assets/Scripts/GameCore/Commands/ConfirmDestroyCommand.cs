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
            var events = new List<GameEvent>();
            var s = state;

            var destroyedLevel = s.CurrentLevel;
            var destroyedName = s.CurrentSword.Name;

            // Fragment reward [P2에서 활성화]
            // var fragmentBonus = context.MasteryTable.GetLevel(s.PlayerData.Mastery.Level).FragmentBonus;
            // var fragResult = new FragmentLogic().GiveFragments(s, destroyedLevel, fragmentBonus);
            // s = fragResult.NewState; events.AddRange(fragResult.Events);

            // Reset to wooden sword
            s = new GameSessionLogic().ResetToWoodenSword(s, context.SwordTable);
            events.Add(new DestroyConfirmedEvent(destroyedLevel, destroyedName));

            return new CommandResult(s, events);
        }
    }
}
