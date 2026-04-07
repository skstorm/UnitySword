using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GameCore.Models;

namespace ConsoleApp
{
    public class SaveManager
    {
        private readonly string _savePath;

        public SaveManager(string savePath = null)
        {
            _savePath = savePath ?? Path.Combine(AppContext.BaseDirectory, "save.json");
        }

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

        public PlayerData Load()
        {
            if (!File.Exists(_savePath))
                return null;

            try
            {
                var json = File.ReadAllText(_savePath);
                var saveData = JsonSerializer.Deserialize<SaveData>(json);

                if (saveData == null)
                {
                    Console.WriteLine("  [!] 저장 파일이 손상되어 새로 시작합니다.");
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
                Console.WriteLine("  [!] 저장 파일이 손상되어 새로 시작합니다.");
                return null;
            }
        }

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
