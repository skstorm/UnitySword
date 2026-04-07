using System;

namespace GameCore.Events;

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
