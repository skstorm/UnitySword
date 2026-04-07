using System.Collections.Generic;
using GameCore.Models;

namespace GameCore.Data
{
    public class MasteryDataTable
    {
        private readonly Dictionary<int, MasteryLevel> _levels;

        public int MaxLevel { get; }

        public MasteryDataTable(List<MasteryLevel> levels)
        {
            _levels = new Dictionary<int, MasteryLevel>();
            foreach (var level in levels)
                _levels[level.Level] = level;
            MaxLevel = levels.Count > 0 ? levels[levels.Count - 1].Level : 0;
        }

        public MasteryLevel GetLevel(int level)
        {
            return _levels.TryGetValue(level, out var result) ? result : null;
        }
    }
}
