namespace GameCore.Events
{
    public class CollectEvent : GameEvent
    {
        public int SwordLevel { get; }
        public string SwordName { get; }
        public bool IsFirstCollect { get; }
        public int UniqueCount { get; }
        public int TotalCollectible { get; }

        public CollectEvent(int swordLevel, string swordName, bool isFirstCollect,
            int uniqueCount, int totalCollectible)
        {
            SwordLevel = swordLevel;
            SwordName = swordName;
            IsFirstCollect = isFirstCollect;
            UniqueCount = uniqueCount;
            TotalCollectible = totalCollectible;
        }
    }

    public class CollectionCompleteEvent : GameEvent
    {
    }
}
