namespace GameCore.Models;

public class Sword
{
    public int Level { get; }
    public string Name { get; }
    public string Theme { get; }
    public double SuccessRate { get; }
    public int EnhanceCost { get; }
    public int CumulativeCost { get; }
    public int SellPrice { get; }
    public int FragmentReward { get; }
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
