using System;

namespace GameCore.Models
{
    public class PlayerData
    {
        // P1
        public int Gold { get; }
        public Statistics Stats { get; }

        // P2
        public int Fragments { get; }
        public MasteryData Mastery { get; }
        public CollectionData Collection { get; }
        public Inventory Inventory { get; }
        public AdLimits AdLimits { get; }

        // P3
        public AchievementData Achievements { get; }
        public TitleData Titles { get; }
        public string Nickname { get; }

        // Meta
        public bool IsFirstRun { get; }
        public DateTime LastSyncedAt { get; }

        public PlayerData(
            int gold = 0,
            Statistics stats = null,
            int fragments = 0,
            MasteryData mastery = null,
            CollectionData collection = null,
            Inventory inventory = null,
            AdLimits adLimits = null,
            AchievementData achievements = null,
            TitleData titles = null,
            string nickname = null,
            bool isFirstRun = true,
            DateTime lastSyncedAt = default)
        {
            Gold = gold;
            Stats = stats ?? new Statistics();
            Fragments = fragments;
            Mastery = mastery ?? new MasteryData();
            Collection = collection ?? new CollectionData();
            Inventory = inventory ?? new Inventory();
            AdLimits = adLimits ?? new AdLimits();
            Achievements = achievements ?? new AchievementData();
            Titles = titles ?? new TitleData();
            Nickname = nickname;
            IsFirstRun = isFirstRun;
            LastSyncedAt = lastSyncedAt;
        }

        public PlayerData With(
            int? gold = null,
            Statistics stats = null,
            int? fragments = null,
            MasteryData mastery = null,
            CollectionData collection = null,
            Inventory inventory = null,
            AdLimits adLimits = null,
            AchievementData achievements = null,
            TitleData titles = null,
            string nickname = null,
            bool? isFirstRun = null,
            DateTime? lastSyncedAt = null)
        {
            return new PlayerData(
                gold ?? Gold,
                stats ?? Stats,
                fragments ?? Fragments,
                mastery ?? Mastery,
                collection ?? Collection,
                inventory ?? Inventory,
                adLimits ?? AdLimits,
                achievements ?? Achievements,
                titles ?? Titles,
                nickname ?? Nickname,
                isFirstRun ?? IsFirstRun,
                lastSyncedAt ?? LastSyncedAt);
        }
    }
}
