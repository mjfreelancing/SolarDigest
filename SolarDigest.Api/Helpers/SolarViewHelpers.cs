using SolarDigest.Api.Extensions;
using SolarDigest.Api.Models;
using System;
using System.Collections.Generic;

namespace SolarDigest.Api.Helpers
{
    public static class SolarViewHelpers
    {
        public static IEnumerable<DateRange> GetMonthlyDateRanges(DateTime startDateTime, DateTime endDateTime)
        {
            var startRequestDate = startDateTime;

            do
            {
                var endRequestDate = startRequestDate.GetEndOfMonth();

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