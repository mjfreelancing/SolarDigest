using AllOverIt.Extensions;
using SolarDigest.Models;
using System;
using TimeZoneConverter;

namespace SolarDigest.Api.Extensions
{
    public static class SiteExtensions
    {
        public static DateTime UtcToLocalTime(this ISite site, DateTime utcTime)
        {
            var tzi = TZConvert.GetTimeZoneInfo(site.TimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
        }

        public static DateTime GetLastAggregationDate(this ISite site)
        {
            // returns in site's local date
            return site.LastAggregationDate.IsNullOrEmpty()
                ? site.StartDate.ParseSolarDate().Date
                : site.LastAggregationDate.ParseSolarDate();
        }

        public static DateTime GetLastRefreshDateTime(this ISite site)
        {
            // returns in site's local date
            return site.LastRefreshDateTime.IsNullOrEmpty()
                ? site.StartDate.ParseSolarDate().Date
                : site.LastRefreshDateTime.ParseSolarDateTime().TrimToHour();
        }

        public static DateTime GetLastSummaryDate(this ISite site)
        {
            // returns in site's local date
            return site.LastSummaryDate.IsNullOrEmpty()
                ? site.StartDate.ParseSolarDate()
                : site.LastSummaryDate.ParseSolarDate();
        }
    }
}