using System;

namespace GameCore.Events;

public class SellEvent : GameEvent
{
    public string SwordName { get; }
    public int Level { get; }
    public int Gold { get; }

    public SellEvent(string swordName, int level, int gold, DateTime timestamp = default)
        : base(timestamp)
    {
        SwordName = swordName;
        Level = level;
        Gold = gold;
    }
}

public class GoldChangeEvent : GameEvent
{
    public int Amount { get; }
    public int NewTotal { get; }
    public string Reason { get; }

    public GoldChangeEvent(int amount, int newTotal, string reason, DateTime timestamp = default)
        : base(timestamp)
    {
        Amount = amount;
        NewTotal = newTotal;
        Reason = reason;
    }
}

public class CollectEvent : GameEvent
{
    public string SwordName { get; }
    public int Level { get; }

    public CollectEvent(string swordName, int level, DateTime timestamp = default)
        : base(timestamp)
    {
        SwordName = swordName;
        Level = level;
    }
}
