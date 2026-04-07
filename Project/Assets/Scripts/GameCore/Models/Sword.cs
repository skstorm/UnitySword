namespace GameCore.Models;

/// <summary>
/// 강화 레벨별 검 데이터. swords.csv에서 로드되는 불변 객체.
/// </summary>
public class Sword
{
    /// <summary>강화 레벨 (0 = 나무검, 20 = 최대).</summary>
    public int Level { get; }
    /// <summary>검 이름 (한글).</summary>
    public string Name { get; }
    /// <summary>테마 분류. Unity UI 연출용으로 예약. (현재 CUI에서 미사용)</summary>
    public string Theme { get; }
    /// <summary>이 레벨로 강화할 때의 성공률 (0.0~1.0).</summary>
    public double SuccessRate { get; }
    /// <summary>이 레벨로 강화하는 데 필요한 골드.</summary>
    public int EnhanceCost { get; }
    /// <summary>이 레벨까지의 누적 투자 골드. Unity 통계 표시용으로 예약.</summary>
    public int CumulativeCost { get; }
    /// <summary>이 검의 판매가.</summary>
    public int SellPrice { get; }
    /// <summary>파괴 시 지급되는 파편 수.</summary>
    public int FragmentReward { get; }
    /// <summary>컬렉션 등록 가능 여부 (+10 이상).</summary>
    public bool Collectable { get; }

    public Sword(
        int level,
        string name,
        string theme,
        double successRate,
        int enhanceCost,
        int cumulativeCost,
        int sellPrice,
        int fragmentReward,
        bool collectable)
    {
        Level = level;
        Name = name;
        Theme = theme;
        SuccessRate = successRate;
        EnhanceCost = enhanceCost;
        CumulativeCost = cumulativeCost;
        SellPrice = sellPrice;
        FragmentReward = fragmentReward;
        Collectable = collectable;
    }
}
