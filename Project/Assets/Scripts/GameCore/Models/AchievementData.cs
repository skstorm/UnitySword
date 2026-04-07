using System.Collections.Generic;

namespace GameCore.Models
{
    public class AchievementData
    {
        public IReadOnlyCollection<string> Achieved { get; }

        public AchievementData(HashSet<string> achieved = null)
        {
            Achieved = achieved ?? new HashSet<string>();
        }

        public AchievementData WithAchieved(string achievementId)
        {
            var newAchieved = new HashSet<string>(Achieved) { achievementId };
            return new AchievementData(newAchieved);
        }
    }
}
