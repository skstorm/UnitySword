using GameCore.Engine;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands;

public class CollectCommand : Command
{
    public override string? Validate(GameState state, GameContext context)
    {
        if (state.PendingAdProtection) return "pending_ad_protection";
        if (state.CurrentLevel < 10) return "level_too_low";
        if (!state.CurrentSword.Collectable) return "not_collectable";
        return null;
    }

    public override CommandResult Execute(GameState state, GameContext context)
    {
        var events = new List<GameEvent>();
        var sword = state.CurrentSword;

        events.Add(new CollectEvent(sword.Name, state.CurrentLevel));

        // Reset to wooden sword
        var newState = GameSessionLogic.ResetToWoodenSword(state, context.SwordTable);

        return new CommandResult(newState, events);
    }
}
