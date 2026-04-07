namespace GameCore.Events
{
    public class EnhanceSuccessEvent : GameEvent
    {
        public int PrevLevel { get; }
        public int NewLevel { get; }
        public string NewSwordName { get; }
        public int GoldSpent { get; }

        public EnhanceSuccessEvent(int prevLevel, int newLevel, string newSwordName, int goldSpent)
        {
            PrevLevel = prevLevel;
            NewLevel = newLevel;
            NewSwordName = newSwordName;
            GoldSpent = goldSpent;
        }
    }

    public class EnhanceFailEvent : GameEvent
    {
        public int DestroyedLevel { get; }
        public string DestroyedSwordName { get; }
        public int FragmentsGained { get; }
        public int GoldSpent { get; }
        public bool AdProtectionAvailable { get; }
        public bool Destroyed { get; }

        public EnhanceFailEvent(int destroyedLevel, string destroyedSwordName,
            int fragmentsGained, int goldSpent, bool adProtectionAvailable, bool destroyed)
        {
            DestroyedLevel = destroyedLevel;
            DestroyedSwordName = destroyedSwordName;
            FragmentsGained = fragmentsGained;
            GoldSpent = goldSpent;
            AdProtectionAvailable = adProtectionAvailable;
            Destroyed = destroyed;
        }
    }
}
