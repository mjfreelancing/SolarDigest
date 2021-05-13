using SolarDigest.Models;
using System;
using TimeZoneConverter;

namespace SolarDigest.Api.Extensions
{
    public static class SiteExtensions
    {
        public static DateTime UtcToLocalTime(this Site site, DateTime utcTime)
        {
            var tzi = TZConvert.GetTimeZoneInfo(site.TimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
        }
    }
}