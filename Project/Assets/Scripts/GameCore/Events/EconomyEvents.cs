namespace GameCore.Events
{
    public class GoldChangeEvent : GameEvent
    {
        public int Amount { get; }
        public int NewTotal { get; }
        public string Reason { get; }

        public GoldChangeEvent(int amount, int newTotal, string reason)
        {
            Amount = amount;
            NewTotal = newTotal;
            Reason = reason;
        }
    }

    public class SellEvent : GameEvent
    {
        public int SoldLevel { get; }
        public string SoldSwordName { get; }
        public int GoldGained { get; }

        public SellEvent(int soldLevel, string soldSwordName, int goldGained)
        {
            SoldLevel = soldLevel;
            SoldSwordName = soldSwordName;
            GoldGained = goldGained;
        }
    }
}
