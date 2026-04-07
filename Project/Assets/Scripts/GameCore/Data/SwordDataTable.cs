using System.Collections.Generic;
using System.Linq;
using GameCore.Models;

namespace GameCore.Data
{
    public class SwordDataTable
    {
        private readonly Dictionary<int, Sword> _swords = new Dictionary<int, Sword>();

        public SwordDataTable(IEnumerable<Sword> swords)
        {
            foreach (var sword in swords)
                _swords[sword.Level] = sword;
        }

        public Sword GetSword(int level)
        {
            return _swords.TryGetValue(level, out var sword) ? sword : null;
        }

        public int MaxLevel => _swords.Count > 0 ? _swords.Keys.Max() : 0;

        public int CollectibleCount => _swords.Values.Count(s => s.Collectible);
    }
}
