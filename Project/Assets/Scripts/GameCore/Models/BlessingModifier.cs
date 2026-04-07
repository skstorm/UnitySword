namespace GameCore.Models;

/// <summary>축복 주문서 효과. 강화 성공률에 고정 보너스를 더한다.</summary>
public class BlessingModifier : IModifier
{
    /// <summary>성공률에 더해지는 보너스 (기본 0.05 = +5%).</summary>
    public double BonusRate { get; }

    public BlessingModifier(double bonusRate = 0.05)
    {
        BonusRate = bonusRate;
    }

    public double Apply(double rate)
    {
        return rate + BonusRate;
    }
}
