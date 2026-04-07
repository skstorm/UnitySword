using System;
using System.Collections.Generic;
using System.Globalization;
using GameCore.Models;

namespace GameCore.Data
{
    public class MasteryDataLoader
    {
        public List<MasteryLevel> Parse(string csv)
        {
            var levels = new List<MasteryLevel>();
            var lines = csv.Split('\n');

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var cols = line.Split(',');
                if (cols.Length < 6) continue;

                try
                {
                    var level = int.Parse(cols[0].Trim());
                    var requiredExp = int.Parse(cols[1].Trim());
                    var costDiscount = double.Parse(cols[2].Trim(), CultureInfo.InvariantCulture);
                    var fragmentBonus = int.Parse(cols[3].Trim());
                    var visualId = cols[4].Trim();
                    var rewardDescription = cols[5].Trim();

                    levels.Add(new MasteryLevel(level, requiredExp, costDiscount,
                        fragmentBonus, visualId, rewardDescription));
                }
                catch (FormatException)
                {
                    // Skip malformed lines
                }
            }

            return levels;
        }
    }
}
