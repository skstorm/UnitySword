using System.Collections.Generic;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Commands;

public enum UseItemType
{
    ProtectionAmulet,
    BlessingScroll,
    GoldPouch
}

public class UseItemCommand : Command
{
    public UseItemType ItemType { get; }

    public UseItemCommand(UseItemType itemType)
    {
        ItemType = itemType;
    }

    public override string Validate(GameState state, GameContext context)
    {
        var items = state.PlayerData.Items;

        switch (ItemType)
        {
            case UseItemType.ProtectionAmulet:
                if (items.ProtectionAmulets <= 0) return "no_protection_amulets";
                if (state.HasActiveProtection) return "already_protected";
                return null;

            case UseItemType.BlessingScroll:
                if (items.BlessingScrolls <= 0) return "no_blessing_scrolls";
                return null;

            case UseItemType.GoldPouch:
                if (items.GoldPouches <= 0) return "no_gold_pouches";
                return null;

            default:
                return "unknown_item";
        }
    }

    public override CommandResult Execute(GameState state, GameContext context)
    {
        var items = state.PlayerData.Items;
        var events = new List<GameEvent>();
        GameState newState;

        switch (ItemType)
        {
            case UseItemType.ProtectionAmulet:
                newState = state.With(
                    hasActiveProtection: true,
                    playerData: state.PlayerData.With(
                        items: items.With(protectionAmulets: items.ProtectionAmulets - 1)));
                events.Add(new ItemUsedEvent("보호 부적", "파괴 방지 활성화"));
                break;

            case UseItemType.BlessingScroll:
                var modifiers = new List<IModifier>(state.ActiveModifiers)
                {
                    new BlessingModifier(0.05)
                };
                newState = state.With(
                    activeModifiers: modifiers,
                    playerData: state.PlayerData.With(
                        items: items.With(blessingScrolls: items.BlessingScrolls - 1)));
                events.Add(new ItemUsedEvent("축복 주문서", "성공률 +5%"));
                break;

            case UseItemType.GoldPouch:
                var newGold = state.PlayerData.Gold + 500;
                newState = state.With(
                    playerData: state.PlayerData.With(
                        gold: newGold,
                        items: items.With(goldPouches: items.GoldPouches - 1)));
                events.Add(new ItemUsedEvent("골드 주머니", "+500G"));
                events.Add(new GoldChangeEvent(500, newGold, "gold_pouch"));
                break;

            default:
                newState = state;
                break;
        }

        return new CommandResult(newState, events);
    }

    public static string GetItemName(UseItemType type) => type switch
    {
        UseItemType.ProtectionAmulet => "보호 부적",
        UseItemType.BlessingScroll => "축복 주문서",
        UseItemType.GoldPouch => "골드 주머니",
        _ => "???"
    };
}
