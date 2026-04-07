using System.Collections.Generic;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Commands
{
    public class UseItemCommand : Command
    {
        public override string Validate(GameState state, GameContext context)
        {
            if (state.PendingAdProtection) return "pending_ad_protection";
            if (state.HasActiveProtection) return "already_protected";
            if (state.PlayerData.Inventory.ProtectionAmulets <= 0) return "no_amulets";
            if (state.CurrentLevel == 0) return "cannot_protect_wooden_sword";
            return null;
        }

        public override CommandResult Execute(GameState state, GameContext context)
        {
            var events = new List<GameEvent>();
            var s = state;

            var newAmulets = s.PlayerData.Inventory.ProtectionAmulets - 1;
            s = s.With(
                hasActiveProtection: true,
                playerData: s.PlayerData.With(
                    inventory: s.PlayerData.Inventory.With(protectionAmulets: newAmulets)));

            events.Add(new UseItemEvent(ItemTypes.ProtectionAmulet, newAmulets));

            return new CommandResult(s, events);
        }
    }
}
