using System;

namespace SolarDigest.Shared.Extensions
{
    public static class DateTimeExtensions
    {
        public static long ToEpoch(this DateTime dateTime)
        {
            var timespan = dateTime - DateTime.UnixEpoch;
            return (long)Math.Floor(timespan.TotalSeconds);
        }
    }
}