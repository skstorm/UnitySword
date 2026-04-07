using System;

namespace GameCore.Events;

/// <summary>강화 성공 이벤트. UI에서 성공 연출 및 다음 강화 비용 표시에 사용.</summary>
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

/// <summary>
/// 강화 실패 이벤트. 파괴 여부, 광고 보호 가능 여부, 파편 보상 정보를 포함.
/// Destroyed=false이면 보호 부적이 발동한 것.
/// </summary>
public class EnhanceFailEvent : GameEvent
{
    public int Level { get; }
    public string SwordName { get; }
    /// <summary>파괴된 검의 판매가 (도발 메시지 표시용).</summary>
    public int SellPrice { get; }
    /// <summary>파괴 시 지급되는 파편 수.</summary>
    public int FragmentReward { get; }
    /// <summary>광고 보호(Unity 전용)를 제안할 수 있는지 여부.</summary>
    public bool AdProtectionAvailable { get; }
    /// <summary>실제로 파괴되었는지 (false면 보호 부적 발동).</summary>
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
