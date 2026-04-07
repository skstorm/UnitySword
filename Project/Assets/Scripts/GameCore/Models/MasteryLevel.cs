namespace GameCore.Models
{
    public class MasteryLevel
    {
        public int Level { get; }
        public int RequiredExp { get; }
        public double CostDiscount { get; }
        public int FragmentBonus { get; }
        public string VisualId { get; }
        public string RewardDescription { get; }

        public MasteryLevel(int level, int requiredExp, double costDiscount,
            int fragmentBonus, string visualId, string rewardDescription)
        {
            Level = level;
            RequiredExp = requiredExp;
            CostDiscount = costDiscount;
            FragmentBonus = fragmentBonus;
            VisualId = visualId;
            RewardDescription = rewardDescription;
        }
    }
}
