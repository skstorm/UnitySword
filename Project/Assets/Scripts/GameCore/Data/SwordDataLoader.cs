using System;
using System.Collections.Generic;
using System.Globalization;
using GameCore.Models;

namespace GameCore.Data
{
    public class SwordDataLoader
    {
        public List<Sword> Parse(string csvContent)
        {
            var swords = new List<Sword>();
            var lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                try
                {
                    var sword = ParseLine(line);
                    if (sword != null)
                        swords.Add(sword);
                }
                catch
                {
                    // Skip malformed lines
                }
            }

            return swords;
        }

        private Sword ParseLine(string line)
        {
            var cols = line.Split(',');
            if (cols.Length < 10) return null;

            var levelStr = cols[0].Trim().TrimStart('+');
            if (!int.TryParse(levelStr, out var level)) return null;

            var name = cols[1].Trim();
            var theme = cols[2].Trim();

            double successRate = 0;
            int enhanceCost = 0;
            int totalInvestment = 0;
            int sellPrice = 0;
            double returnRate = 0;
            int fragmentReward = 0;

            if (cols[3].Trim() != "-")
                double.TryParse(cols[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out successRate);
            successRate /= 100.0; // CSV is percentage, convert to 0.0~1.0

            if (cols[4].Trim() != "-")
                int.TryParse(cols[4].Trim(), out enhanceCost);

            if (cols[5].Trim() != "-")
                int.TryParse(cols[5].Trim(), out totalInvestment);

            if (cols[6].Trim() != "-")
                int.TryParse(cols[6].Trim(), out sellPrice);

            var returnRateStr = cols[7].Trim().TrimEnd('%');
            if (returnRateStr != "-")
            {
                if (double.TryParse(returnRateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var rr))
                    returnRate = rr / 100.0;
            }

            if (cols[8].Trim() != "-")
                int.TryParse(cols[8].Trim(), out fragmentReward);

            var collectible = cols[9].Trim().Equals("Y", StringComparison.OrdinalIgnoreCase);

            return new Sword(level, name, theme, successRate, enhanceCost, totalInvestment,
                sellPrice, returnRate, fragmentReward, collectible);
        }
    }
}
