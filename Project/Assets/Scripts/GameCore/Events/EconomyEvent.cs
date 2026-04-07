using System;

namespace GameCore.Events;

/// <summary>검 판매 이벤트.</summary>
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

/// <summary>골드 변동 이벤트. 강화 비용, 판매 수익, 지원금 등 모든 골드 변동에 사용.</summary>
public class GoldChangeEvent : GameEvent
{
    /// <summary>변동량 (음수면 지출).</summary>
    public int Amount { get; }
    /// <summary>변동 후 총 골드.</summary>
    public int NewTotal { get; }
    /// <summary>변동 사유 코드 (예: "enhance_cost", "sell_sword").</summary>
    public string Reason { get; }

    public GoldChangeEvent(int amount, int newTotal, string reason, DateTime timestamp = default)
        : base(timestamp)
    {
        Amount = amount;
        NewTotal = newTotal;
        Reason = reason;
    }
}

/// <summary>컬렉션 등록 이벤트.</summary>
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
