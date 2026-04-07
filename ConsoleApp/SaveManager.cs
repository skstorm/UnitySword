using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GameCore.Models;

namespace ConsoleApp
{
    /// <summary>
    /// 파일 시스템 기반 PlayerData 저장/로드 관리자.
    /// PlayerData를 JSON DTO(SaveData)로 변환하여 save.json에 기록한다.
    /// Unity 등 다른 플랫폼에서는 이 클래스를 대체하여 자체 저장소(PlayerPrefs 등)를 사용한다.
    /// </summary>
    public class SaveManager
    {
        private readonly string _savePath;

        /// <summary>마지막 Load에서 파일 손상이 감지되었는지 여부.</summary>
        public bool LastLoadCorrupted { get; private set; }

        public SaveManager(string savePath = null)
        {
            _savePath = savePath ?? Path.Combine(AppContext.BaseDirectory, "save.json");
        }

        /// <summary>PlayerData를 JSON 파일로 직렬화하여 저장한다.</summary>
        public void Save(PlayerData data)
        {
            var saveData = new SaveData
            {
                Gold = data.Gold,
                Fragments = data.Fragments,
                MasteryLevel = data.Mastery.Level,
                MasteryTotalAttempts = data.Mastery.TotalAttempts,
                ProtectionAmulets = data.Inventory.ProtectionAmulets,
                Collected = new Dictionary<int, int>(data.Collection.Collected),
                TotalCollectible = data.Collection.TotalCollectible,
                HighestEnhanceLevel = data.Stats.HighestEnhanceLevel,
                WeeklyHighestLevel = data.Stats.WeeklyHighestLevel,
                TotalDestroys = data.Stats.TotalDestroys,
                TotalEnhanceAttempts = data.Stats.TotalEnhanceAttempts,
                TotalSells = data.Stats.TotalSells,
                TotalGoldEarned = data.Stats.TotalGoldEarned,
                TotalGoldSpent = data.Stats.TotalGoldSpent,
                MaxConsecutiveSuccess = data.Stats.MaxConsecutiveSuccess,
                MaxConsecutiveFail = data.Stats.MaxConsecutiveFail
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(saveData, options);
            File.WriteAllText(_savePath, json);
        }

        /// <summary>
        /// JSON 파일에서 PlayerData를 역직렬화하여 로드한다.
        /// 파일이 없거나 손상된 경우 null을 반환하며, LastLoadCorrupted로 손상 여부를 확인할 수 있다.
        /// </summary>
        public PlayerData Load()
        {
            LastLoadCorrupted = false;

            if (!File.Exists(_savePath))
                return null;

            try
            {
                var json = File.ReadAllText(_savePath);
                var saveData = JsonSerializer.Deserialize<SaveData>(json);

                if (saveData == null)
                {
                    LastLoadCorrupted = true;
                    return null;
                }

                var statistics = new Statistics(
                    highestEnhanceLevel: saveData.HighestEnhanceLevel,
                    weeklyHighestLevel: saveData.WeeklyHighestLevel,
                    totalDestroys: saveData.TotalDestroys,
                    totalEnhanceAttempts: saveData.TotalEnhanceAttempts,
                    totalSells: saveData.TotalSells,
                    totalGoldEarned: saveData.TotalGoldEarned,
                    totalGoldSpent: saveData.TotalGoldSpent,
                    maxConsecutiveSuccess: saveData.MaxConsecutiveSuccess,
                    maxConsecutiveFail: saveData.MaxConsecutiveFail
                );

                var mastery = new MasteryData(
                    level: saveData.MasteryLevel,
                    totalAttempts: saveData.MasteryTotalAttempts
                );

                var inventory = new Inventory(
                    protectionAmulets: saveData.ProtectionAmulets
                );

                var collection = new CollectionData(
                    collected: saveData.Collected ?? new Dictionary<int, int>(),
                    totalCollectible: saveData.TotalCollectible
                );

                return new PlayerData(
                    gold: saveData.Gold,
                    stats: statistics,
                    fragments: saveData.Fragments,
                    mastery: mastery,
                    collection: collection,
                    inventory: inventory,
                    isFirstRun: false
                );
            }
            catch (JsonException)
            {
                LastLoadCorrupted = true;
                return null;
            }
        }

        /// <summary>JSON 직렬화용 DTO. PlayerData의 평탄화된 스냅샷.</summary>
        private class SaveData
        {
            public int Gold { get; set; }
            public int Fragments { get; set; }
            public int MasteryLevel { get; set; }
            public int MasteryTotalAttempts { get; set; }
            public int ProtectionAmulets { get; set; }
            public Dictionary<int, int> Collected { get; set; }
            public int TotalCollectible { get; set; }
            public int HighestEnhanceLevel { get; set; }
            public int WeeklyHighestLevel { get; set; }
            public int TotalDestroys { get; set; }
            public int TotalEnhanceAttempts { get; set; }
            public int TotalSells { get; set; }
            public int TotalGoldEarned { get; set; }
            public int TotalGoldSpent { get; set; }
            public int MaxConsecutiveSuccess { get; set; }
            public int MaxConsecutiveFail { get; set; }
        }
    }
}
