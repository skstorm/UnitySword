using System.Collections.Generic;
using GameCore.Models;

namespace GameCore.Data;

/// <summary>레벨별 검 데이터를 담는 조회 테이블. swords.csv에서 로드된다.</summary>
public class SwordDataTable
{
    private readonly Dictionary<int, Sword> _swords;

    /// <summary>데이터에 존재하는 최대 강화 레벨.</summary>
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
