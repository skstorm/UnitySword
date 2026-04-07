using System;

namespace GameCore.Events;

public class EnhanceSuccessEvent : GameEvent
{
    public int PreviousLevel { get; }
    public int NewLevel { get; }
    public string SwordName { get; }
    public int NextEnhanceCost { get; }

    public EnhanceSuccessEvent(int previousLevel, int newLevel, string swordName, int nextEnhanceCost, DateTime timestamp = default)
        : base(timestamp)
    {
        PreviousLevel = previousLevel;
        NewLevel = newLevel;
        SwordName = swordName;
        NextEnhanceCost = nextEnhanceCost;
    }
}

public class EnhanceFailEvent : GameEvent
{
    public int Level { get; }
    public string SwordName { get; }
    public int SellPrice { get; }
    public int FragmentReward { get; }
    public bool AdProtectionAvailable { get; }
    public bool Destroyed { get; }

    public EnhanceFailEvent(int level, string swordName, int sellPrice, int fragmentReward, bool adProtectionAvailable, bool destroyed, DateTime timestamp = default)
        : base(timestamp)
    {
        Level = level;
        SwordName = swordName;
        SellPrice = sellPrice;
        FragmentReward = fragmentReward;
        AdProtectionAvailable = adProtectionAvailable;
        Destroyed = destroyed;
    }
}
