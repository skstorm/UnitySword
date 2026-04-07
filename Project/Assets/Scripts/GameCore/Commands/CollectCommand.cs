using System.Collections.Generic;
using System.Linq;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands;

public class CollectCommand : Command
{
    public override string Validate(GameState state, GameContext context)
    {
        if (state.PendingAdProtection) return "pending_ad_protection";
        if (state.CurrentLevel < 10) return "level_too_low";
        if (!state.CurrentSword.Collectable) return "not_collectable";
        if (state.PlayerData.CollectedSwords.Contains(state.CurrentLevel))
            return "already_collected";
        return null;
    }

    public override CommandResult Execute(GameState state, GameContext context)
    {
        var events = new List<GameEvent>();
        var sword = state.CurrentSword;
        var level = state.CurrentLevel;

        events.Add(new CollectEvent(sword.Name, level));

        // Add to collected swords
        var collected = new List<int>(state.PlayerData.CollectedSwords) { level };
        var newPlayerData = state.PlayerData.With(collectedSwords: collected);
        var stateWithCollection = state.With(playerData: newPlayerData);

        // Reset to wooden sword
        var newState = GameSessionLogic.ResetToWoodenSword(stateWithCollection, context.SwordTable);

        return new CommandResult(newState, events);
    }
}
