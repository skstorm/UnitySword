using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GameCore.Models;

namespace GameCore.Data;

public static class SwordDataLoader
{
    public static SwordDataTable Load(string csvPath)
    {
        var swords = new Dictionary<int, Sword>();
        var lines = File.ReadAllLines(csvPath);

        if (lines.Length < 2)
            return new SwordDataTable(swords);

        // Parse header to find column indices
        var header = SplitCsvLine(lines[0]);
        int colLevel = FindColumn(header, "강화");
        int colName = FindColumn(header, "검 이름");
        int colTheme = FindColumn(header, "테마");
        int colRate = FindColumn(header, "성공률(%)");
        int colCost = FindColumn(header, "강화비용");
        int colCumCost = FindColumn(header, "누적투자");
        int colSell = FindColumn(header, "판매가");
        int colFragment = FindColumn(header, "파괴시파편");
        int colCollect = FindColumn(header, "수집가능");

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            var cols = SplitCsvLine(line);

            int level = ParseLevel(GetCol(cols, colLevel));
            string name = GetCol(cols, colName);
            string theme = GetCol(cols, colTheme);
            double successRate = ParseRate(GetCol(cols, colRate));
            int enhanceCost = ParseIntOrZero(GetCol(cols, colCost));
            int cumulativeCost = ParseIntOrZero(GetCol(cols, colCumCost));
            int sellPrice = ParseIntOrZero(GetCol(cols, colSell));
            int fragmentReward = ParseIntOrZero(GetCol(cols, colFragment));
            bool collectable = ParseBool(GetCol(cols, colCollect));

            var sword = new Sword(level, name, theme, successRate, enhanceCost, cumulativeCost, sellPrice, fragmentReward, collectable);
            swords[level] = sword;
        }

        return new SwordDataTable(swords);
    }

    private static string[] SplitCsvLine(string line)
    {
        return line.Split(',');
    }

    private static int FindColumn(string[] header, string name)
    {
        for (int i = 0; i < header.Length; i++)
        {
            if (header[i].Trim() == name)
                return i;
        }
        return -1;
    }

    private static string GetCol(string[] cols, int index)
    {
        if (index < 0 || index >= cols.Length)
            return "-";
        return cols[index].Trim();
    }

    private static int ParseLevel(string value)
    {
        if (string.IsNullOrEmpty(value) || value == "-")
            return 0;
        value = value.Replace("+", "");
        if (int.TryParse(value, out int result))
            return result;
        return 0;
    }

    private static double ParseRate(string value)
    {
        if (string.IsNullOrEmpty(value) || value == "-")
            return 0.0;
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
            return result / 100.0;
        return 0.0;
    }

    private static int ParseIntOrZero(string value)
    {
        if (string.IsNullOrEmpty(value) || value == "-")
            return 0;
        value = value.Replace(",", "");
        if (int.TryParse(value, out int result))
            return result;
        return 0;
    }

    private static bool ParseBool(string value)
    {
        return value == "Y";
    }
}
