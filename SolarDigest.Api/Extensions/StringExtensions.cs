using System;

namespace SolarDigest.Api.Extensions
{
    public static class StringExtensions
    {
        public static DateTime ParseSolarDate(this string timestamp)
        {
            return DateTime.ParseExact(timestamp, Constants.DateFormat, null);
        }

        public static DateTime ParseSolarDateTime(this string timestamp)
        {
            return DateTime.ParseExact(timestamp, Constants.DateTimeFormat, null);
        }

        public static string NormaliseEnumValue(this string value)
        {
            return value.Replace("_", string.Empty);
        }
    }
}