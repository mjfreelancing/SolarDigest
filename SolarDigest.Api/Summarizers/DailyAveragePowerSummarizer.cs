using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System;
using System.Collections.Generic;

namespace SolarDigest.Api.Summarizers
{
    internal sealed class DailyAveragePowerSummarizer : PowerSummarizerBase, IDailyAveragePowerSummarizer
    {
        public DailyAveragePowerSummarizer(ISolarDigestPowerTable dailyTable, ISolarDigestPowerMonthlyTable monthlyTable,
            ISolarDigestPowerYearlyTable yearlyTable)
            : base(dailyTable, monthlyTable, yearlyTable)
        {
        }

        protected override IEnumerable<TimeWatts> GetMeterReadings(IDictionary<string, IList<PeriodWattData>> daily,
            IDictionary<string, IList<PeriodWattData>> monthly, IDictionary<string, IList<PeriodWattData>> yearly)
        {
            var meterReadings = new List<TimeWatts>();

            for (var minutes = 0; minutes < 24 * 4 * 15; minutes += 15)
            {
                var timespan = TimeSpan.FromMinutes(minutes);
                var time = $"{timespan.Hours:D2}{timespan.Minutes:D2}";

                var wattValues = new List<PeriodWattData>();

                if (daily.ContainsKey(time))
                {
                    wattValues.AddRange(daily[time]);
                }

                if (monthly.ContainsKey(time))
                {
                    wattValues.AddRange(monthly[time]);
                }

                if (yearly.ContainsKey(time))
                {
                    wattValues.AddRange(yearly[time]);
                }

                var totalDays = 0;
                var totalWatts = 0.0d;
                var totalWattHour = 0.0d;

                foreach (var wattItem in wattValues)
                {
                    totalDays += wattItem.DayCount;
                    totalWatts += wattItem.Watts;
                    totalWattHour += wattItem.WattHour;
                }

                var formattedTime = $"{timespan.Hours:D2}:{timespan.Minutes:D2}";
                var averageWatts = totalDays == 0 ? 0.0d : totalWatts / totalDays;
                var averageWattHour = totalDays == 0 ? 0.0d : totalWattHour / totalDays;

                var timeWatts = new TimeWatts(
                    formattedTime,
                    Math.Round(averageWatts, 6, MidpointRounding.AwayFromZero),
                    Math.Round(averageWattHour, 6, MidpointRounding.AwayFromZero));

                meterReadings.Add(timeWatts);
            }

            return meterReadings;
        }
    }
}