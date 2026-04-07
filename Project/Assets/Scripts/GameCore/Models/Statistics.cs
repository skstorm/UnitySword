namespace GameCore.Models
{
    public class Statistics
    {
        public int HighestEnhanceLevel { get; }
        public int TotalDestroys { get; }
        public int TotalEnhanceAttempts { get; }
        public int TotalSells { get; }
        public int TotalGoldEarned { get; }
        public int MaxConsecutiveSuccess { get; }
        public int MaxConsecutiveFail { get; }
        public int CurrentConsecutiveSuccess { get; }
        public int CurrentConsecutiveFail { get; }

        public Statistics(
            int highestEnhanceLevel = 0,
            int totalDestroys = 0,
            int totalEnhanceAttempts = 0,
            int totalSells = 0,
            int totalGoldEarned = 0,
            int maxConsecutiveSuccess = 0,
            int maxConsecutiveFail = 0,
            int currentConsecutiveSuccess = 0,
            int currentConsecutiveFail = 0)
        {
            HighestEnhanceLevel = highestEnhanceLevel;
            TotalDestroys = totalDestroys;
            TotalEnhanceAttempts = totalEnhanceAttempts;
            TotalSells = totalSells;
            TotalGoldEarned = totalGoldEarned;
            MaxConsecutiveSuccess = maxConsecutiveSuccess;
            MaxConsecutiveFail = maxConsecutiveFail;
            CurrentConsecutiveSuccess = currentConsecutiveSuccess;
            CurrentConsecutiveFail = currentConsecutiveFail;
        }

        public Statistics With(
            int? highestEnhanceLevel = null,
            int? totalDestroys = null,
            int? totalEnhanceAttempts = null,
            int? totalSells = null,
            int? totalGoldEarned = null,
            int? maxConsecutiveSuccess = null,
            int? maxConsecutiveFail = null,
            int? currentConsecutiveSuccess = null,
            int? currentConsecutiveFail = null)
        {
            return new Statistics(
                highestEnhanceLevel ?? HighestEnhanceLevel,
                totalDestroys ?? TotalDestroys,
                totalEnhanceAttempts ?? TotalEnhanceAttempts,
                totalSells ?? TotalSells,
                totalGoldEarned ?? TotalGoldEarned,
                maxConsecutiveSuccess ?? MaxConsecutiveSuccess,
                maxConsecutiveFail ?? MaxConsecutiveFail,
                currentConsecutiveSuccess ?? CurrentConsecutiveSuccess,
                currentConsecutiveFail ?? CurrentConsecutiveFail);
        }
    }
}
