using System.Collections.Generic;

namespace GameCore.Models
{
    public class TitleData
    {
        public HashSet<string> Earned { get; }
        public string Equipped { get; }

        public TitleData(HashSet<string> earned = null, string equipped = null)
        {
            Earned = earned ?? new HashSet<string>();
            Equipped = equipped;
        }
    }
}
