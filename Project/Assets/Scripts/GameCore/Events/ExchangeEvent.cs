using System;

namespace GameCore.Events;

/// <summary>파편 교환 이벤트. 어떤 아이템을 몇 파편에 교환했는지 기록.</summary>
public class ExchangeEvent : GameEvent
{
    public string ItemName { get; }
    public int FragmentCost { get; }
    public int RemainingFragments { get; }

    public ExchangeEvent(string itemName, int fragmentCost, int remainingFragments, DateTime timestamp = default)
        : base(timestamp)
    {
        ItemName = itemName;
        FragmentCost = fragmentCost;
        RemainingFragments = remainingFragments;
    }
}

/// <summary>아이템 사용 이벤트. 사용한 아이템과 적용된 효과 설명을 포함.</summary>
public class ItemUsedEvent : GameEvent
{
    public string ItemName { get; }
    public string Effect { get; }

    public ItemUsedEvent(string itemName, string effect, DateTime timestamp = default)
        : base(timestamp)
    {
        ItemName = itemName;
        Effect = effect;
    }
}
