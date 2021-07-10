using System;

namespace SolarDigest.Shared.Utils
{
    public static class DateTimeUtils
    {
        public static DateTime ToDateTimeFromEpoch(long epoch)
        {
            return DateTime.UnixEpoch.AddSeconds(epoch);
        }
    }
}