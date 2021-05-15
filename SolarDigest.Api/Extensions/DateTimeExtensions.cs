using SolarDigest.Api.Models;
using System;
using System.Collections.Generic;

namespace SolarDigest.Api.Extensions
{
    public static class DateTimeExtensions
    {
        public static string GetSolarDateString(this DateTime timestamp)
        {
            return $"{timestamp:yyyy-MM-dd}";
        }

        public static string GetSolarDateTimeString(this DateTime timestamp)
        {
            return $"{timestamp:yyyy-MM-dd HH:mm:ss}";
        }

        public static DateTime TrimToHour(this DateTime dateTime)
        {
            return new(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);
        }

        public static DateTime TrimToDayOfMonth(this DateTime dateTime, int day)
        {
            return new(dateTime.Year, dateTime.Month, day, 0, 0, 0);
        }

        public static DateTime GetEndOfMonth(this DateTime dateTime)
        {
            return new DateTime(
                dateTime.Year, dateTime.Month,
                DateTime.DaysInMonth(dateTime.Year, dateTime.Month), 23, 59, 59);
        }

        public static bool IsSameMonthYear(this DateTime dateTime, DateTime other)
        {
            return dateTime.Year == other.Year && dateTime.Month == other.Month;
        }

        public static IEnumerable<DateRange> GetWeeklyDateRangesUntil(this DateTime startDateTime, DateTime endDateTime)
        {
            var startRequestDate = startDateTime;

            do
            {
                var endRequestDate = startRequestDate.AddDays(6);

                if (endRequestDate > endDateTime)
                {
                    endRequestDate = endDateTime;
                }

                yield return new DateRange(startRequestDate, endRequestDate);

                startRequestDate = endRequestDate.AddDays(1).Date;

            } while (startRequestDate < endDateTime);
        }
    }
}