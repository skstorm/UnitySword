namespace GameCore.Models
{
    public class Sword
    {
        public int Level { get; }
        public string Name { get; }
        public string Theme { get; }
        public double SuccessRate { get; }
        public int EnhanceCost { get; }
        public int TotalInvestment { get; }
        public int SellPrice { get; }
        public double ReturnRate { get; }
        public int FragmentReward { get; }
        public bool Collectible { get; }

        public Sword(int level, string name, string theme, double successRate,
            int enhanceCost, int totalInvestment, int sellPrice, double returnRate,
            int fragmentReward, bool collectible)
        {
            Level = level;
            Name = name;
            Theme = theme;
            SuccessRate = successRate;
            EnhanceCost = enhanceCost;
            TotalInvestment = totalInvestment;
            SellPrice = sellPrice;
            ReturnRate = returnRate;
            FragmentReward = fragmentReward;
            Collectible = collectible;
        }
    }
}
