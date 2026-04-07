namespace GameCore.Models
{
    public class MasteryData
    {
        public int Level { get; }
        public int TotalAttempts { get; }

        public MasteryData(int level = 1, int totalAttempts = 0)
        {
            Level = level;
            TotalAttempts = totalAttempts;
        }

        public MasteryData With(int? level = null, int? totalAttempts = null)
        {
            return new MasteryData(
                level ?? Level,
                totalAttempts ?? TotalAttempts);
        }
    }
}
