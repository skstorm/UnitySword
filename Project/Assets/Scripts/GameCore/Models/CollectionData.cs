using System.Collections.Generic;
using System.Linq;

namespace GameCore.Models
{
    public class CollectionData
    {
        public Dictionary<int, int> Collected { get; }
        public int UniqueCount => Collected.Count;
        public int TotalCollectible { get; }
        public double CompletionRate => TotalCollectible > 0 ? (double)UniqueCount / TotalCollectible : 0;

        public CollectionData(Dictionary<int, int> collected = null, int totalCollectible = 11)
        {
            Collected = collected ?? new Dictionary<int, int>();
            TotalCollectible = totalCollectible;
        }

        public CollectionData WithCollected(int swordLevel)
        {
            var newCollected = new Dictionary<int, int>(Collected);
            if (newCollected.ContainsKey(swordLevel))
                newCollected[swordLevel]++;
            else
                newCollected[swordLevel] = 1;
            return new CollectionData(newCollected, TotalCollectible);
        }
    }
}
