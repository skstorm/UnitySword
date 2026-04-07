using System.Collections.Generic;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Commands;

/// <summary>인벤토리의 아이템을 사용하는 커맨드.</summary>
public enum UseItemType
{
    ProtectionAmulet,  // 보호 부적
    BlessingScroll,    // 축복 주문서
    GoldPouch          // 골드 주머니
}

public class UseItemCommand : Command
{
    public UseItemType ItemType { get; }

    public UseItemCommand(UseItemType itemType)
    {
        ItemType = itemType;
    }

    /// <summary>해당 아이템을 보유하고 있는지, 사용 조건을 만족하는지 검증한다.</summary>
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

    /// <summary>아이템을 소모하고 효과를 적용한다.</summary>
    public override CommandResult Execute(GameState state, GameContext context)
    {
        var items = state.PlayerData.Items;
        var events = new List<GameEvent>();
        GameState newState;

        switch (ItemType)
        {
            // 보호 부적: 다음 강화 실패 시 파괴를 방지
            case UseItemType.ProtectionAmulet:
                newState = state.With(
                    hasActiveProtection: true,
                    playerData: state.PlayerData.With(
                        items: items.With(protectionAmulets: items.ProtectionAmulets - 1)));
                events.Add(new ItemUsedEvent(GameConstants.ProtectionAmuletName, "파괴 방지 활성화"));
                break;

            // 축복 주문서: 성공률 보너스 버프 적용
            case UseItemType.BlessingScroll:
                var modifiers = new List<IModifier>(state.ActiveModifiers)
                {
                    new BlessingModifier(GameConstants.BlessingScrollBonus)
                };
                newState = state.With(
                    activeModifiers: modifiers,
                    playerData: state.PlayerData.With(
                        items: items.With(blessingScrolls: items.BlessingScrolls - 1)));
                events.Add(new ItemUsedEvent(GameConstants.BlessingScrollName, "성공률 +5%"));
                break;

            // 골드 주머니: 즉시 골드 지급
            case UseItemType.GoldPouch:
                var newGold = state.PlayerData.Gold + GameConstants.GoldPouchGoldAmount;
                newState = state.With(
                    playerData: state.PlayerData.With(
                        gold: newGold,
                        items: items.With(goldPouches: items.GoldPouches - 1)));
                events.Add(new ItemUsedEvent(GameConstants.GoldPouchName, $"+{GameConstants.GoldPouchGoldAmount}G"));
                events.Add(new GoldChangeEvent(GameConstants.GoldPouchGoldAmount, newGold, "gold_pouch"));
                break;

            default:
                newState = state;
                break;
        }

        return new CommandResult(newState, events);
    }

    /// <summary>아이템 종류의 한글 표시명을 반환한다. ExchangeCommand.GetItemName과 동일 결과.</summary>
    public static string GetItemName(UseItemType type) => type switch
    {
        UseItemType.ProtectionAmulet => GameConstants.ProtectionAmuletName,
        UseItemType.BlessingScroll => GameConstants.BlessingScrollName,
        UseItemType.GoldPouch => GameConstants.GoldPouchName,
        _ => "???"
    };
}
