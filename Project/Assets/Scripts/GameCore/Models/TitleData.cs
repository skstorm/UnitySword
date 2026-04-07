using System.Collections.Generic;

namespace GameCore.Models
{
    public class TitleData
    {
        public IReadOnlyCollection<string> Earned { get; }
        public string Equipped { get; }

        public TitleData(HashSet<string> earned = null, string equipped = null)
        {
            Earned = earned ?? new HashSet<string>();
            Equipped = equipped;
        }

        public TitleData WithEarned(string titleId)
        {
            var newEarned = new HashSet<string>(Earned) { titleId };
            return new TitleData(newEarned, Equipped);
        }

        public TitleData With(string equipped)
        {
            return new TitleData(new HashSet<string>(Earned), equipped);
        }
    }
}
