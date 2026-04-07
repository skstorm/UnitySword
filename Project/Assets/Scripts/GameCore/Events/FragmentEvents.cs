namespace GameCore.Events
{
    public class FragmentGainEvent : GameEvent
    {
        public int BaseAmount { get; }
        public int BonusAmount { get; }
        public int TotalGained { get; }
        public int NewTotal { get; }
        public int FromLevel { get; }

        public FragmentGainEvent(int baseAmount, int bonusAmount, int totalGained,
            int newTotal, int fromLevel)
        {
            BaseAmount = baseAmount;
            BonusAmount = bonusAmount;
            TotalGained = totalGained;
            NewTotal = newTotal;
            FromLevel = fromLevel;
        }
    }

    public class ExchangeEvent : GameEvent
    {
        public string ItemType { get; }
        public int FragmentCost { get; }
        public int RemainingFragments { get; }

        public ExchangeEvent(string itemType, int fragmentCost, int remainingFragments)
        {
            ItemType = itemType;
            FragmentCost = fragmentCost;
            RemainingFragments = remainingFragments;
        }
    }

    public class UseItemEvent : GameEvent
    {
        public string ItemType { get; }
        public int RemainingCount { get; }

        public UseItemEvent(string itemType, int remainingCount)
        {
            ItemType = itemType;
            RemainingCount = remainingCount;
        }
    }
}
