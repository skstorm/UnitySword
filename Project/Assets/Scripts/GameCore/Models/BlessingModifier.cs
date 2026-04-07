namespace GameCore.Models;

public class BlessingModifier : IModifier
{
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
