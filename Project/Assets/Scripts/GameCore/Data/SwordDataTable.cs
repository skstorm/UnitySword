using System.Collections.Generic;
using GameCore.Models;

namespace GameCore.Data;

public class SwordDataTable
{
    private readonly Dictionary<int, Sword> _swords;

    public int MaxLevel { get; }

    public SwordDataTable(Dictionary<int, Sword> swords)
    {
        _swords = swords;
        MaxLevel = 0;
        foreach (var kvp in _swords)
        {
            if (kvp.Key > MaxLevel)
                MaxLevel = kvp.Key;
        }
    }

    public Sword GetSword(int level)
    {
        _swords.TryGetValue(level, out var sword);
        return sword;
    }
}
