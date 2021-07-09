using AllOverIt.Extensions;
using AllOverIt.Helpers;
using AllOverIt.Tasks;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SolarDigest.Api.Summarizers
{
    internal abstract class PowerSummarizerBase : IPowerSummarizer
    {
        private static readonly DateRange ExcludedNoDates = new(DateTime.MaxValue, DateTime.MinValue);

        // creates a new instance - avoid any possibility of something being added later that affects logic
        private static IDictionary<string, IList<PeriodWattData>> EmptyMeterReadings => new Dictionary<string, IList<PeriodWattData>>();

        private static readonly CultureInfo CultureInfo = new(Constants.AggregationOptions.CultureName);
        private static Calendar Calendar => CultureInfo.Calendar;

        private readonly ISolarDigestPowerTable _dailyTable;
        private readonly ISolarDigestPowerMonthlyTable _monthlyTable;
        private readonly ISolarDigestPowerYearlyTable _yearlyTable;
        private readonly ISolarDigestLogger _logger;

        protected PowerSummarizerBase(ISolarDigestPowerTable dailyTable, ISolarDigestPowerMonthlyTable monthlyTable,
            ISolarDigestPowerYearlyTable yearlyTable, ISolarDigestLogger logger)
        {
            _dailyTable = dailyTable.WhenNotNull(nameof(dailyTable));
            _monthlyTable = monthlyTable.WhenNotNull(nameof(monthlyTable));
            _yearlyTable = yearlyTable.WhenNotNull(nameof(yearlyTable));
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task<IEnumerable<TimeWatts>> GetTimeWattsAsync(string siteId, MeterType meterType, DateTime startDate, DateTime endDate)
        {
            // determine what full years and months we have, then the remaining individual days

            _logger.LogDebug("Getting day, month, and year periods");

            var yearPeriods = GetYearPeriods(startDate, endDate).AsReadOnlyList();
            var monthPeriods = GetMonthPeriods(startDate, endDate, yearPeriods).AsReadOnlyList();
            var dayPeriods = GetDayPeriods(startDate, endDate, yearPeriods, monthPeriods);

            _logger.LogDebug("Reading daily, monthly, and yearly meter data");

            var (daily, monthly, yearly) = await TaskHelper
                .WhenAll(
                    GetDailyReadings(siteId, meterType, dayPeriods),
                    GetMonthlyReadings(siteId, meterType, monthPeriods),
                    GetYearlyReadings(siteId, meterType, yearPeriods))
                .ConfigureAwait(false);

            _logger.LogDebug("Get summarized meter readings");

            return GetMeterSummary(daily, monthly, yearly);
        }

        protected abstract IEnumerable<TimeWatts> GetMeterSummary(
            IDictionary<string, IList<PeriodWattData>> daily,
            IDictionary<string, IList<PeriodWattData>> monthly,
            IDictionary<string, IList<PeriodWattData>> yearly);

        private static IEnumerable<AggregationYear> GetYearPeriods(DateTime startDate, DateTime endDate)
        {
            var yearStartDate = startDate.Day == 1 && startDate.Month == 12
                ? startDate
                : new DateTime(startDate.Year + 1, 1, 1);

            var yearEndDate = new DateTime(yearStartDate.Year, 12, 31);

            while (yearStartDate < endDate && yearEndDate <= endDate)
            {
                yield return new AggregationYear(yearStartDate, yearEndDate, yearStartDate.Year);

                yearStartDate = yearStartDate.AddYears(1);
                yearEndDate = yearEndDate.AddYears(1);
            }
        }

        private static IEnumerable<AggregationMonth> GetMonthPeriods(DateTime startDate, DateTime endDate,
            IReadOnlyCollection<AggregationYear> yearPeriods)
        {
            var excludeDateRange = yearPeriods.Count == 0
                ? ExcludedNoDates
                : new DateRange(yearPeriods.First().StartDate, yearPeriods.Last().EndDate);

            var monthStartDate = startDate.Day == 1
                ? startDate
                : new DateTime(startDate.Year, startDate.Month + 1, 1);

            static DateTime GetLastDayOfMonth(DateTime date)
            {
                var daysInMonth = Calendar.GetDaysInMonth(date.Year, date.Month);
                return new DateTime(date.Year, date.Month, daysInMonth);
            }

            var monthEndDate = GetLastDayOfMonth(monthStartDate);

            while (monthStartDate < endDate && monthEndDate <= endDate)
            {
                if (!(monthStartDate >= excludeDateRange.StartDateTime && monthEndDate <= excludeDateRange.EndDateTime))
                {
                    yield return new AggregationMonth(monthStartDate, monthEndDate, monthStartDate.Year, monthStartDate.Month);
                }

                monthStartDate = monthStartDate.AddMonths(1);
                monthEndDate = GetLastDayOfMonth(monthStartDate);
            }
        }

        private static IReadOnlyCollection<DateRange> GetDayPeriods(in DateTime startDate, in DateTime endDate,
            IReadOnlyCollection<AggregationYear> yearPeriods, IReadOnlyCollection<AggregationMonth> monthPeriods)
        {
            var minDateCovered = DateTime.MaxValue;
            var maxDateCovered = DateTime.MinValue;

            if (yearPeriods.Count > 0)
            {
                minDateCovered = yearPeriods.First().StartDate;
                maxDateCovered = yearPeriods.Last().EndDate;
            }

            if (monthPeriods.Count > 0)
            {
                minDateCovered = monthPeriods.First().StartDate;
                maxDateCovered = monthPeriods.Last().EndDate;
            }

            var dayPeriods = new List<DateRange>();

            if (minDateCovered == DateTime.MaxValue)
            {
                dayPeriods.Add(new DateRange(startDate, endDate));
            }
            else
            {
                dayPeriods.Add(new DateRange(startDate, minDateCovered.AddDays(-1)));
                dayPeriods.Add(new DateRange(maxDateCovered.AddDays(1), endDate));
            }

            return dayPeriods;
        }

        private async Task<IDictionary<string, IList<PeriodWattData>>> GetDailyReadings(string siteId, MeterType meterType,
            IReadOnlyCollection<DateRange> dayPeriods)
        {
            if (dayPeriods.Count == 0)
            {
                return EmptyMeterReadings;
            }

            var meterReadings = new Dictionary<string, IList<PeriodWattData>>();

            foreach (var dayPeriod in dayPeriods)
            {
                for (var date = dayPeriod.StartDateTime; date <= dayPeriod.EndDateTime; date = date.AddDays(1))
                {
                    _logger.LogDebug($"Getting daily {meterType} data for {date.GetSolarDateString()}");

                    var dailyPowerItems = _dailyTable.GetMeterPowerAsync(siteId, date, meterType);

                    await foreach (var powerItem in dailyPowerItems.ConfigureAwait(false))
                    {
                        if (!meterReadings.ContainsKey(powerItem.Time))
                        {
                            meterReadings.Add(powerItem.Time, new List<PeriodWattData>());
                        }

                        meterReadings[powerItem.Time].Add(new PeriodWattData(1, powerItem.Watts, powerItem.WattHour));
                    }
                }
            }

            _logger.LogDebug("Daily readings have been read");

            return meterReadings;
        }

        private async Task<IDictionary<string, IList<PeriodWattData>>> GetMonthlyReadings(string siteId, MeterType meterType,
            IReadOnlyCollection<AggregationMonth> monthPeriods)
        {
            if (monthPeriods.Count == 0)
            {
                return EmptyMeterReadings;
            }

            var meterReadings = new Dictionary<string, IList<PeriodWattData>>();

            foreach (var monthPeriod in monthPeriods)
            {
                _logger.LogDebug($"Getting monthly {meterType} data between {monthPeriod.StartDate.GetSolarDateString()} and  " +
                                 $"{monthPeriod.EndDate.GetSolarDateString()}");

                var monthlyPowerItems = _monthlyTable.GetMeterDataAsync(siteId, monthPeriod.Year, monthPeriod.Month, meterType);

                await foreach (var powerItem in monthlyPowerItems.ConfigureAwait(false))
                {
                    if (!meterReadings.ContainsKey(powerItem.Time))
                    {
                        meterReadings.Add(powerItem.Time, new List<PeriodWattData>());
                    }

                    meterReadings[powerItem.Time].Add(new PeriodWattData(powerItem.DayCount, powerItem.Watts, powerItem.WattHour));
                }
            }

            _logger.LogDebug("Monthly readings have been read");

            return meterReadings;
        }

        private async Task<IDictionary<string, IList<PeriodWattData>>> GetYearlyReadings(string siteId, MeterType meterType,
            IReadOnlyCollection<AggregationYear> yearPeriods)
        {
            if (yearPeriods.Count == 0)
            {
                return EmptyMeterReadings;
            }

            var meterReadings = new Dictionary<string, IList<PeriodWattData>>();

            foreach (var yearPeriod in yearPeriods)
            {
                _logger.LogDebug($"Getting monthly {meterType} data between {yearPeriod.StartDate.GetSolarDateString()} and  " +
                                 $"{yearPeriod.EndDate.GetSolarDateString()}");

                var yearlyPowerItems = _yearlyTable.GetMeterDataAsync(siteId, yearPeriod.Year, meterType);

                await foreach (var powerItem in yearlyPowerItems.ConfigureAwait(false))
                {
                    if (!meterReadings.ContainsKey(powerItem.Time))
                    {
                        meterReadings.Add(powerItem.Time, new List<PeriodWattData>());
                    }

                    meterReadings[powerItem.Time].Add(new PeriodWattData(powerItem.DayCount, powerItem.Watts, powerItem.WattHour));
                }
            }

            _logger.LogDebug("Yearly readings have been read");

            return meterReadings;
        }
    }
}