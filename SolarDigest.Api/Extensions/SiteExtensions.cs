using SolarDigest.Models;
using System;

namespace SolarDigest.Api.Extensions
{
    public static class SiteExtensions
    {
        public static DateTime UtcToLocalTime(this Site site, DateTime utcTime)
        {
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(site.TimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
        }
    }
}