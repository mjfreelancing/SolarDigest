using AllOverIt.Helpers;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models;
using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarDigest.Api.Aggregators
{
    internal sealed class PowerYearlyAggregator : IPowerYearlyAggregator
    {
        private readonly ISolarDigestPowerTable _powerTable;
        private readonly ISolarDigestPowerYearlyTable _powerYearlyTable;
        private readonly IFunctionLogger _logger;

        public PowerYearlyAggregator(ISolarDigestPowerTable powerTable, ISolarDigestPowerYearlyTable powerYearlyTable, IFunctionLogger logger)
        {
            _powerTable = powerTable.WhenNotNull(nameof(powerTable));
            _powerYearlyTable = powerYearlyTable.WhenNotNull(nameof(powerYearlyTable));
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task ProcessAsync(Site site, DateTime startDate, DateTime endDate)
        {
            _logger.LogDebug($"Processing yearly aggregation for site {site.Id} between {startDate.GetSolarDateString()} and {endDate.GetSolarDateString()}");

            var siteStartDate = site.StartDate.ParseSolarDate();

            void LogSkippingDatesPriorToSiteStartDate(DateTime skipStartDate, DateTime skipEndDate)
            {
                _logger.LogDebug($"Skipping yearly aggregation between {skipStartDate.GetSolarDateString()} and {skipEndDate.GetSolarDateString()} for " +
                                 $"site {site.Id} (prior to the site start date of {siteStartDate.GetSolarDateString()})");
            }

            IEnumerable<Task> GetYearlyTasks()
            {
                for (var year = startDate.Year; year <= endDate.Year; year++)
                {
                    var yearStartDate = new DateTime(year, 1, 1);
                    var yearEndDate = new DateTime(year, 12, 31);

                    if (yearEndDate < siteStartDate)
                    {
                        LogSkippingDatesPriorToSiteStartDate(yearStartDate, yearEndDate);
                        continue;
                    }

                    // the first/last year may not be a complete year
                    if (yearStartDate < siteStartDate)
                    {
                        LogSkippingDatesPriorToSiteStartDate(yearStartDate, siteStartDate.AddDays(-1));
                        yearStartDate = siteStartDate;
                    }

                    if (yearEndDate > endDate)
                    {
                        yearEndDate = endDate;
                    }

                    var daysToCollect = (yearEndDate - yearStartDate).Days + 1;

                    foreach (var meterType in EnumHelper.GetEnumValues<MeterType>())
                    {
                        _logger.LogDebug($"Aggregating yearly {meterType} data for site {site.Id} between {yearStartDate.GetSolarDateString()} " +
                                         $"and {yearEndDate.GetSolarDateString()}");

                        yield return PersistAggregatedMeterValues(site.Id, meterType, yearStartDate, daysToCollect);
                    }
                }
            }

            // running these (and others) all in parallel was too much for the free tier in AWS so they are being processed sequentially
            await GetYearlyTasks()
                .InvokeTasksSequentially()
                .ConfigureAwait(false);
        }

        private async Task PersistAggregatedMeterValues(string siteId, MeterType meterType, DateTime startDate, int daysToCollect)
        {
            var timeWatts = new Dictionary<string, (double Watts, double WattHour)>();

            for (var dayOffset = 0; dayOffset < daysToCollect; dayOffset++)
            {
                var date = startDate.AddDays(dayOffset);

                var meterEntities = _powerTable.GetMeterPowerAsync(siteId, date, meterType);

                await foreach (var entity in meterEntities)
                {
                    // Note: can't seem to use TryGetValue() or GetValueOrDefault() with tuples without
                    // complaining about possible null reference
                    var (watts, wattHour) = (0.0d, 0.0d);

                    if (timeWatts.ContainsKey(entity.Time))
                    {
                        (watts, wattHour) = timeWatts[entity.Time];
                    }

                    var totalWatts = watts + entity.Watts;
                    var totalWattHour = wattHour + entity.WattHour;

                    timeWatts[entity.Time] = (totalWatts, totalWattHour);
                }
            }

            // this will be prior to the actual last day of the week if it is a partial week
            var endDate = startDate.AddDays(daysToCollect - 1);

            var powerData = timeWatts.Select(kvp =>
            {
                var time = kvp.Key;
                var (watts, wattHour) = kvp.Value;

                return new MeterPowerYear(siteId, startDate, endDate, time, meterType, watts, wattHour);
            });

            await _powerYearlyTable.AddMeterPowerAsync(powerData).ConfigureAwait(false);
        }
    }
}