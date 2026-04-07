using System;

namespace GameCore.Models
{
    public class AdLimits
    {
        public int AdProtectionUsedToday { get; }
        public DateTime LastResetDate { get; }

        public AdLimits(int adProtectionUsedToday = 0, DateTime lastResetDate = default)
        {
            AdProtectionUsedToday = adProtectionUsedToday;
            LastResetDate = lastResetDate;
        }

        public AdLimits With(int? adProtectionUsedToday = null, DateTime? lastResetDate = null)
        {
            return new AdLimits(
                adProtectionUsedToday ?? AdProtectionUsedToday,
                lastResetDate ?? LastResetDate);
        }
    }
}
