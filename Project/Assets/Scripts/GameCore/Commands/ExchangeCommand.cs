using System.Collections.Generic;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands;

/// <summary>파편으로 아이템을 교환하는 커맨드.</summary>
public enum ExchangeItemType
{
    ProtectionAmulet,  // 보호 부적
    BlessingScroll,    // 축복 주문서
    GoldPouch          // 골드 주머니
}

public class ExchangeCommand : Command
{
    public ExchangeItemType ItemType { get; }

    public ExchangeCommand(ExchangeItemType itemType)
    {
        ItemType = itemType;
    }

    /// <summary>파편이 충분한지 검증한다.</summary>
    public override string Validate(GameState state, GameContext context)
    {
        var cost = GetFragmentCost();
        if (state.PlayerData.Fragments < cost)
            return "insufficient_fragments";
        return null;
    }

    /// <summary>파편을 차감하고 해당 아이템을 인벤토리에 추가한다.</summary>
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
                events.Add(new ExchangeEvent(GameConstants.ProtectionAmuletName, cost, newFragments));
                break;

            case ExchangeItemType.BlessingScroll:
                newPlayerData = state.PlayerData.With(
                    fragments: newFragments,
                    items: items.With(blessingScrolls: items.BlessingScrolls + 1));
                events.Add(new ExchangeEvent(GameConstants.BlessingScrollName, cost, newFragments));
                break;

            case ExchangeItemType.GoldPouch:
                var newGold = state.PlayerData.Gold + GameConstants.GoldPouchGoldAmount;
                newPlayerData = state.PlayerData.With(
                    fragments: newFragments,
                    gold: newGold);
                events.Add(new ExchangeEvent(GameConstants.GoldPouchName, cost, newFragments));
                events.Add(new GoldChangeEvent(GameConstants.GoldPouchGoldAmount, newGold, "exchange_gold_pouch"));
                break;

            default:
                newPlayerData = state.PlayerData;
                break;
        }

        var newState = state.With(playerData: newPlayerData);
        return new CommandResult(newState, events);
    }

    /// <summary>아이템 종류별 교환에 필요한 파편 수를 반환한다.</summary>
    public int GetFragmentCost() => ItemType switch
    {
        ExchangeItemType.ProtectionAmulet => GameConstants.ProtectionAmuletFragmentCost,
        ExchangeItemType.BlessingScroll => GameConstants.BlessingScrollFragmentCost,
        ExchangeItemType.GoldPouch => GameConstants.GoldPouchFragmentCost,
        _ => 0
    };

    /// <summary>아이템 종류의 한글 표시명을 반환한다.</summary>
    public static string GetItemName(ExchangeItemType type) => type switch
    {
        ExchangeItemType.ProtectionAmulet => GameConstants.ProtectionAmuletName,
        ExchangeItemType.BlessingScroll => GameConstants.BlessingScrollName,
        ExchangeItemType.GoldPouch => GameConstants.GoldPouchName,
        _ => "???"
    };
}
