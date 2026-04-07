using System.Collections.Generic;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands
{
    public class SellCommand : Command
    {
        public override string Validate(GameState state, GameContext context)
        {
            if (state.PendingAdProtection) return "pending_ad_protection";
            if (state.CurrentLevel == 0) return "cannot_sell_wooden_sword";
            return null;
        }

        public override CommandResult Execute(GameState state, GameContext context)
        {
            var events = new List<GameEvent>();
            var s = state;

            var sellPrice = s.CurrentSword.SellPrice;
            var soldLevel = s.CurrentLevel;
            var soldName = s.CurrentSword.Name;

            // Add gold
            var addResult = new EconomyLogic().AddGold(s, sellPrice, "sell");
            s = addResult.NewState;
            events.AddRange(addResult.Events);

            // Update stats
            var newStats = s.PlayerData.Stats.With(
                totalSells: s.PlayerData.Stats.TotalSells + 1);
            s = s.With(playerData: s.PlayerData.With(stats: newStats));

            // Sell event
            events.Add(new SellEvent(soldLevel, soldName, sellPrice));

            // Reset to wooden sword
            s = new GameSessionLogic().ResetToWoodenSword(s, context.SwordTable);

            return new CommandResult(s, events);
        }
    }
}
