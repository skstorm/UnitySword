using System.Collections.Generic;

namespace GameCore.Models
{
    public class AchievementData
    {
        public HashSet<string> Achieved { get; }

        public AchievementData(HashSet<string> achieved = null)
        {
            Achieved = achieved ?? new HashSet<string>();
        }
    }
}
