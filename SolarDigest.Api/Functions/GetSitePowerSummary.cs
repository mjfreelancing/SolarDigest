using AllOverIt.Extensions;
using AllOverIt.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Functions.Validators;
using SolarDigest.Api.Models;
using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{

    /*

    {
      "version" : "2017-02-28",
      "operation": "Invoke",
      "payload": { 
        "id": $context.source.id,
        "meterType": $context.arguments.filter.meterType,
        "summaryType": $context.arguments.filter.summaryType
      }
    }

    */

    // For the purpose of CDK deployment, all functions need to reside in the same (SolarDigest.Api.Functions) namespace.

    public sealed class GetSitePowerSummary : FunctionBase<GetSitePowerSummaryPayload, SitePower>
    {
        private static readonly DateRange ExcludedNoDates = new(DateTime.MaxValue, DateTime.MinValue);

        // creates a new instance - avoid any possibility of something being added later that affects logic
        private static IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>> EmptyMeterReadings =>
            new Dictionary<string, IList<(int DayCount, double Watts, double WattHour)>>();

        private static readonly CultureInfo CultureInfo = new(Constants.AggregationOptions.CultureName);
        private static Calendar Calendar => CultureInfo.Calendar;

        protected override async Task<SitePower> InvokeHandlerAsync(FunctionContext<GetSitePowerSummaryPayload> context)
        {
            var serviceProvider = context.ScopedServiceProvider;
            var payload = context.Payload;

            // AppSync enforces the dates and enum values, but we validate them anyway (along with the site Id)
            serviceProvider.InvokeValidator<GetSitePowerSummaryPayloadValidator, GetSitePowerSummaryPayload>(payload);

            var logger = context.Logger;

            var siteId = payload.SiteId;
            var startDate = payload.StartDate.ParseSolarDate();
            var endDate = payload.EndDate.ParseSolarDate();
            var meterType = payload.MeterType.As<MeterType>();
            var summaryType = payload.SummaryType.As<SummaryType>();

            logger.LogDebug($"Getting power summary ({meterType}, {summaryType}) for site Id " +
                            $"{siteId} between {payload.StartDate} and {payload.EndDate}");

            // determine what full years and months we have, then the remaining individual days
            var yearPeriods = GetYearPeriods(startDate, endDate).AsReadOnlyList();
            var monthPeriods = GetMonthPeriods(startDate, endDate, yearPeriods).AsReadOnlyList();
            var dayPeriods = GetDayPeriods(startDate, endDate, yearPeriods, monthPeriods);

            // load all of the required data in parallel
            var powerTable = serviceProvider.GetRequiredService<ISolarDigestPowerTable>();
            var monthlyTable = serviceProvider.GetRequiredService<ISolarDigestPowerMonthlyTable>();
            var yearlyTable = serviceProvider.GetRequiredService<ISolarDigestPowerYearlyTable>();

            var (daily, monthly, yearly) = await TaskHelper
                .WhenAll(
                    GetDailyReadings(powerTable, siteId, meterType, dayPeriods),
                    GetMonthlyReadings(monthlyTable, siteId, meterType, monthPeriods),
                    GetYearlyReadings(yearlyTable, siteId, meterType, yearPeriods))
                .ConfigureAwait(false);

            logger.LogDebug($"{daily.Count} daily items read");
            logger.LogDebug($"{monthly.Count} monthly items read");
            logger.LogDebug($"{yearly.Count} yearly items read");

            var meterReadings = GetMeterReadings(daily, monthly, yearly);

            return new SitePower(meterReadings);
        }

        private static IEnumerable<TimeWatts> GetMeterReadings(
          IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>> daily,
          IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>> monthly,
          IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>> yearly)
        {
            var meterReadings = new List<TimeWatts>();

            for (var minutes = 0; minutes < 24 * 4 * 15; minutes += 15)
            {
                var timespan = TimeSpan.FromMinutes(minutes);
                var time = $"{timespan.Hours:D2}{timespan.Minutes:D2}";

                var wattValues = new List<(int DayCount, double Watts, double WattHour)>();

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

                foreach (var (dayCount, watts, wattHour) in wattValues)
                {
                    totalDays += dayCount;
                    totalWatts += watts;
                    totalWattHour += wattHour;
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

        private static async Task<IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>>>
            GetDailyReadings(ISolarDigestPowerTable powerTable, string siteId, MeterType meterType, IReadOnlyCollection<DateRange> dayPeriods)
        {
            if (dayPeriods.Count == 0)
            {
                return EmptyMeterReadings;
            }

            var meterReadings = new Dictionary<string, IList<(int DayCount, double Watts, double WattHour)>>();

            foreach (var dayPeriod in dayPeriods)
            {
                for (var date = dayPeriod.StartDateTime; date <= dayPeriod.EndDateTime; date = date.AddDays(1))
                {
                    await foreach (var powerItem in powerTable.GetMeterPowerAsync(siteId, date, meterType).ConfigureAwait(false))
                    {
                        if (!meterReadings.ContainsKey(powerItem.Time))
                        {
                            meterReadings.Add(powerItem.Time, new List<(int, double, double)>());
                        }

                        meterReadings[powerItem.Time].Add((1, powerItem.Watts, powerItem.WattHour));
                    }
                }
            }

            return meterReadings;
        }

        private static async Task<IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>>>
            GetMonthlyReadings(ISolarDigestPowerMonthlyTable monthlyTable, string siteId, MeterType meterType, IReadOnlyCollection<AggregationMonth> monthPeriods)
        {
            if (monthPeriods.Count == 0)
            {
                return EmptyMeterReadings;
            }

            var meterReadings = new Dictionary<string, IList<(int DayCount, double Watts, double WattHour)>>();

            foreach (var monthPeriod in monthPeriods)
            {
                await foreach (var powerItem in monthlyTable.GetMeterDataAsync(siteId, monthPeriod.Year, monthPeriod.Month, meterType).ConfigureAwait(false))
                {
                    if (!meterReadings.ContainsKey(powerItem.Time))
                    {
                        meterReadings.Add(powerItem.Time, new List<(int, double, double)>());
                    }

                    meterReadings[powerItem.Time].Add((powerItem.DayCount, powerItem.Watts, powerItem.WattHour));
                }
            }

            return meterReadings;
        }

        private static async Task<IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>>>
            GetYearlyReadings(ISolarDigestPowerYearlyTable yearlyTable, string siteId, MeterType meterType, IReadOnlyCollection<AggregationYear> yearPeriods)
        {
            if (yearPeriods.Count == 0)
            {
                return EmptyMeterReadings;
            }

            var meterReadings = new Dictionary<string, IList<(int DayCount, double Watts, double WattHour)>>();

            foreach (var yearPeriod in yearPeriods)
            {
                await foreach (var powerItem in yearlyTable.GetMeterDataAsync(siteId, yearPeriod.Year, meterType).ConfigureAwait(false))
                {
                    if (!meterReadings.ContainsKey(powerItem.Time))
                    {
                        meterReadings.Add(powerItem.Time, new List<(int, double, double)>());
                    }

                    meterReadings[powerItem.Time].Add((powerItem.DayCount, powerItem.Watts, powerItem.WattHour));
                }
            }

            return meterReadings;
        }

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

        private static IEnumerable<AggregationMonth> GetMonthPeriods(DateTime startDate, DateTime endDate, IReadOnlyCollection<AggregationYear> yearPeriods)
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
    }
}