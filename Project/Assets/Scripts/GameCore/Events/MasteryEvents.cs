namespace GameCore.Events
{
    public class MasteryExpEvent : GameEvent
    {
        public int NewTotalAttempts { get; }
        public int CurrentLevel { get; }

        public MasteryExpEvent(int newTotalAttempts, int currentLevel)
        {
            NewTotalAttempts = newTotalAttempts;
            CurrentLevel = currentLevel;
        }
    }

    public class MasteryLevelUpEvent : GameEvent
    {
        public int PrevLevel { get; }
        public int NewLevel { get; }
        public string RewardDescription { get; }

        public MasteryLevelUpEvent(int prevLevel, int newLevel, string rewardDescription)
        {
            PrevLevel = prevLevel;
            NewLevel = newLevel;
            RewardDescription = rewardDescription;
        }
    }
}
