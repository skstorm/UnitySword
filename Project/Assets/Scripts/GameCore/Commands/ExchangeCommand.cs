using System.Collections.Generic;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands;

public enum ExchangeItemType
{
    ProtectionAmulet,  // 보호 부적 — 10 파편
    BlessingScroll,    // 축복 주문서 — 15 파편
    GoldPouch          // 골드 주머니 — 5 파편 → +500G
}

public class ExchangeCommand : Command
{
    public ExchangeItemType ItemType { get; }

    public ExchangeCommand(ExchangeItemType itemType)
    {
        ItemType = itemType;
    }

    public override string Validate(GameState state, GameContext context)
    {
        var cost = GetFragmentCost();
        if (state.PlayerData.Fragments < cost)
            return "insufficient_fragments";
        return null;
    }

    public override CommandResult Execute(GameState state, GameContext context)
    {
        var cost = GetFragmentCost();
        var newFragments = state.PlayerData.Fragments - cost;
        var items = state.PlayerData.Items;
        var events = new List<GameEvent>();

        PlayerData newPlayerData;

        switch (ItemType)
        {
            case ExchangeItemType.ProtectionAmulet:
                newPlayerData = state.PlayerData.With(
                    fragments: newFragments,
                    items: items.With(protectionAmulets: items.ProtectionAmulets + 1));
                events.Add(new ExchangeEvent("보호 부적", cost, newFragments));
                break;

            case ExchangeItemType.BlessingScroll:
                newPlayerData = state.PlayerData.With(
                    fragments: newFragments,
                    items: items.With(blessingScrolls: items.BlessingScrolls + 1));
                events.Add(new ExchangeEvent("축복 주문서", cost, newFragments));
                break;

            case ExchangeItemType.GoldPouch:
                var newGold = state.PlayerData.Gold + 500;
                newPlayerData = state.PlayerData.With(
                    fragments: newFragments,
                    gold: newGold);
                events.Add(new ExchangeEvent("골드 주머니", cost, newFragments));
                events.Add(new GoldChangeEvent(500, newGold, "exchange_gold_pouch"));
                break;

            default:
                newPlayerData = state.PlayerData;
                break;
        }

        var newState = state.With(playerData: newPlayerData);
        return new CommandResult(newState, events);
    }

    public int GetFragmentCost() => ItemType switch
    {
        ExchangeItemType.ProtectionAmulet => 10,
        ExchangeItemType.BlessingScroll => 15,
        ExchangeItemType.GoldPouch => 5,
        _ => 0
    };

    public static string GetItemName(ExchangeItemType type) => type switch
    {
        ExchangeItemType.ProtectionAmulet => "보호 부적",
        ExchangeItemType.BlessingScroll => "축복 주문서",
        ExchangeItemType.GoldPouch => "골드 주머니",
        _ => "???"
    };
}
