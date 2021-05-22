using AllOverIt.Helpers;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models;
using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SolarDigest.Api.Processors
{
    internal sealed class PowerMonthlyProcessor : IPowerMonthlyProcessor
    {
        private readonly ISolarDigestPowerTable _powerTable;
        private readonly ISolarDigestPowerMonthlyTable _powerMonthlyTable;
        private readonly IFunctionLogger _logger;

        public PowerMonthlyProcessor(ISolarDigestPowerTable powerTable, ISolarDigestPowerMonthlyTable powerMonthlyTable, IFunctionLogger logger)
        {
            _powerTable = powerTable.WhenNotNull(nameof(powerTable));
            _powerMonthlyTable = powerMonthlyTable.WhenNotNull(nameof(powerMonthlyTable));
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task ProcessAsync(Site site, DateTime startDate, DateTime endDate)
        {
            _logger.LogDebug($"Processing monthly power aggregation for site {site.Id} between {startDate.GetSolarDateString()} and {endDate.GetSolarDateString()}");

            var siteStartDate = site.StartDate.ParseSolarDate();

            var cultureInfo = new CultureInfo(Constants.AggregationOptions.CultureName);
            var calendar = cultureInfo.Calendar;

            void LogSkippingDatesPriorToSiteStartDate(DateTime skipStartDate, DateTime skipEndDate)
            {
                _logger.LogDebug($"Skipping monthly aggregation between {skipStartDate.GetSolarDateString()} and {skipEndDate.GetSolarDateString()} " +
                                 $"for site {site.Id} (prior to the site start date of {siteStartDate.GetSolarDateString()})");
            }

            IEnumerable<Task> GetMonthlyTasks()
            {
                for (var trackingStartDate = startDate.TrimToDayOfMonth(1);
                    trackingStartDate <= endDate;
                    trackingStartDate = trackingStartDate.AddMonths(1))
                {
                    var monthStartDate = trackingStartDate.TrimToDayOfMonth(1);

                    var lastDayInMonth = calendar.GetDaysInMonth(trackingStartDate.Year, trackingStartDate.Month);
                    var monthEndDate = trackingStartDate.TrimToDayOfMonth(lastDayInMonth);

                    if (monthEndDate < siteStartDate)
                    {
                        LogSkippingDatesPriorToSiteStartDate(monthStartDate, monthEndDate);
                        continue;
                    }

                    // the first/last month may not be a complete month
                    if (monthStartDate < siteStartDate)
                    {
                        LogSkippingDatesPriorToSiteStartDate(monthStartDate, siteStartDate.AddDays(-1));
                        monthStartDate = siteStartDate;
                    }

                    var daysToCollect = (monthEndDate - monthStartDate).Days + 1;

                    foreach (var meterType in EnumHelper.GetEnumValues<MeterType>())
                    {
                        _logger.LogDebug($"Aggregating monthly {meterType} data for site {site.Id} between {monthStartDate.GetSolarDateString()} " +
                                         $"and {monthEndDate.GetSolarDateString()}");

                        yield return PersistAggregatedMeterValuesAsync(site.Id, meterType, monthStartDate, daysToCollect);
                    }
                }
            }

            // running these (and others) all in parallel was too much for the free tier in AWS so they are being processed sequentially
            await GetMonthlyTasks()
                .InvokeTasksSequentially()
                .ConfigureAwait(false);

            _logger.LogDebug($"Completed monthly power aggregation for site {site.Id} between {startDate.GetSolarDateString()} and {endDate.GetSolarDateString()}");
        }

        private async Task PersistAggregatedMeterValuesAsync(string siteId, MeterType meterType, DateTime startDate, int daysToCollect)
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

                return new MeterPowerMonth(siteId, startDate, endDate, time, meterType, watts, wattHour);
            });

            await _powerMonthlyTable.AddMeterPowerAsync(powerData).ConfigureAwait(false);
        }
    }
}