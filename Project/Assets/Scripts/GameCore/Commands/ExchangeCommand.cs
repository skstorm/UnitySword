using System.Collections.Generic;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands
{
    public class ExchangeCommand : Command
    {
        public const int ProtectionAmuletCost = 30;
        public const int GoldPouchCost = 10;
        public const int GoldPouchReward = 50;

        public string ItemType { get; }

        public ExchangeCommand(string itemType)
        {
            ItemType = itemType;
        }

        public override string Validate(GameState state, GameContext context)
        {
            if (state.PendingAdProtection) return "pending_ad_protection";

            switch (ItemType)
            {
                case "protection_amulet":
                    if (state.PlayerData.Fragments < ProtectionAmuletCost)
                        return "insufficient_fragments";
                    break;
                case "gold_pouch":
                    if (state.PlayerData.Fragments < GoldPouchCost)
                        return "insufficient_fragments";
                    break;
                default:
                    return "invalid_item_type";
            }

            return null;
        }

        public override CommandResult Execute(GameState state, GameContext context)
        {
            var events = new List<GameEvent>();
            var s = state;

            switch (ItemType)
            {
                case "protection_amulet":
                {
                    var newFragments = s.PlayerData.Fragments - ProtectionAmuletCost;
                    var newAmulets = s.PlayerData.Inventory.ProtectionAmulets + 1;
                    s = s.With(playerData: s.PlayerData.With(
                        fragments: newFragments,
                        inventory: s.PlayerData.Inventory.With(protectionAmulets: newAmulets)));
                    events.Add(new ExchangeEvent("protection_amulet", ProtectionAmuletCost, newFragments));
                    break;
                }
                case "gold_pouch":
                {
                    var newFragments = s.PlayerData.Fragments - GoldPouchCost;
                    s = s.With(playerData: s.PlayerData.With(fragments: newFragments));
                    events.Add(new ExchangeEvent("gold_pouch", GoldPouchCost, newFragments));

                    var addResult = new EconomyLogic().AddGold(s, GoldPouchReward, "gold_pouch");
                    s = addResult.NewState;
                    events.AddRange(addResult.Events);
                    break;
                }
            }

            return new CommandResult(s, events);
        }
    }
}
