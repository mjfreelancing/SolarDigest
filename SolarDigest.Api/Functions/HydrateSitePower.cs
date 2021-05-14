using AllOverIt.Extensions;
using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Helpers;
using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Models.SolarEdgeData;
using SolarDigest.Api.Payloads.EventBridge;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    /*
     
    Test payload to use in the console:

    To perform an update since the last refresh timestamp:

        {
          "detail": {
            "siteId": "1514817"
          }
        }

    To perform an update based on an explicit timestamp range:

        {
          "detail": {
            "siteId": "1514817",
            "startDateTime": "2020-06-01 12:00:00",
            "endDateTime": "2020-06-01 13:00:00"
          }
        }

    */

    public sealed class HydrateSitePower : FunctionBase<HydrateSitePowerPayload, bool>
    {
        protected override async Task<bool> InvokeHandlerAsync(FunctionContext<HydrateSitePowerPayload> context)
        {
            var logger = context.Logger;

            var request = context.Payload.Detail;

            // todo: add request validation

            var siteId = request.SiteId;

            logger.LogDebug($"Hydrating power for site Id '{siteId}'");

            // get site info
            var siteTable = context.ScopedServiceProvider.GetService<ISolarDigestSiteTable>();
            var site = await siteTable!.GetItemAsync<Site>(siteId);


            // determine the refresh period to hydrate (local time)
            // - the start/end date/time are optional - a forced re-fresh can be achieved when providing them
            DateTime hydrateStartDateTime;

            if (request.StartDateTime.IsNullOrEmpty())
            {
                hydrateStartDateTime = site.LastRefreshDateTime.IsNullOrEmpty()
                    ? site.StartDate.ParseSolarDate()
                    : site.LastRefreshDateTime.ParseSolarDateTime();
            }
            else
            {
                hydrateStartDateTime = request.StartDateTime.ParseSolarDateTime();
            }

            var hydrateEndDateTime = request.EndDateTime.IsNullOrEmpty() 
                ? site.UtcToLocalTime(DateTime.UtcNow).AddHours(-1).TrimToHour() 
                : request.EndDateTime.ParseSolarDateTime();

            // todo: validate the start date <= the last refresh timestamp

            logger.LogDebug($"Site '{siteId}' will be hydrating for the period {hydrateStartDateTime.GetSolarDateTimeString()} to " +
                            $"{hydrateEndDateTime.GetSolarDateTimeString()} (local time)");


            // todo: add this
            // await NotifyPowerUpdated(context, PowerUpdatedStatus.Started, triggeredPowerQuery);


            // The solarEdge API can only process, at most, 1 month of data at a time
            var dateRanges = SolarViewHelpers
                .GetMonthlyDateRanges(hydrateStartDateTime, hydrateEndDateTime)
                .AsReadOnlyList();


            foreach (var dateRange in dateRanges)
            {
                logger.LogDebug($"Processing period: {dateRange.StartDateTime.GetSolarDateTimeString()} to {dateRange.EndDateTime.GetSolarDateTimeString()}");

                // Refer to HydratePowerOrchestrator for code...

            }








            //// get the power / energy data
            //var solarEdgeApi = context.ScopedServiceProvider.GetService<ISolarEdgeApi>();

            //var powerQuery = new PowerQuery
            //{
            //    SiteId = siteId,
            //    StartDateTime = hydrateStartDateTime.GetSolarDateTimeString(),
            //    EndDateTime = hydrateEndDateTime.GetSolarDateTimeString()
            //};

            //var (powerResults, energyResults) = await TaskHelper.WhenAll(
            //    solarEdgeApi!.GetPowerDetailsAsync(powerQuery),
            //    solarEdgeApi!.GetEnergyDetailsAsync(powerQuery)
            //);





            //var meterCount = powerResults.PowerDetails.Meters.Count();
            //var energyCount = energyResults.EnergyDetails.Meters.Count();

            //logger.LogDebug($"Received {meterCount} power meter and {energyCount} energy meter results");



            //var mapper = context.ScopedServiceProvider.GetService<IMapper>();

            //var powerData = mapper!.Map<SolarData>(powerResults);
            //var energyData = mapper.Map<SolarData>(energyResults);

            //var solarViewDays = GetSolarViewDays(powerQuery.SiteId, powerData, energyData);

            //foreach (var solarViewDay in solarViewDays)
            //{
            //    var entities = solarViewDay.Meters
            //        .SelectMany(
            //            meter => meter.Points,
            //            (meter, point) => new MeterPowerEntity(solarViewDay.SiteId, point.Timestamp, meter.MeterType, point.Watts, point.WattHour))
            //        .AsReadOnlyList();
            //}







            return true;
        }

        private static IEnumerable<SolarViewDay> GetSolarViewDays(string siteId, SolarData powerData, SolarData energyData)
        {
            // flattened list of data points - so we can group into days and meter types
            var powerMeterPoints =
              from meter in powerData.MeterValues.Meters
              let meterType = meter.Type.As<MeterType>()
              from value in meter.Values
              let timestamp = value.Date.ParseSolarDateTime()
              let watts = value.Value
              select new
              {
                  timestamp.Date,
                  MeterType = meterType,
                  Timestamp = timestamp,
                  Watts = watts
              };

            var energyMeterPoints =
              from meter in energyData.MeterValues.Meters
              let meterType = meter.Type.As<MeterType>()
              from value in meter.Values
              let timestamp = value.Date.ParseSolarDateTime()
              let wattHour = value.Value
              select new
              {
                  timestamp.Date,
                  MeterType = meterType,
                  Timestamp = timestamp,
                  WattHour = wattHour
              };

            var meterPoints = from power in powerMeterPoints
                              join energy in energyMeterPoints
                                on new { power.MeterType, power.Date, power.Timestamp }
                                equals new { energy.MeterType, energy.Date, energy.Timestamp }
                              select new
                              {
                                  power.MeterType,
                                  power.Date,
                                  power.Timestamp,
                                  power.Watts,
                                  WattHour = energy.WattHour
                              };

            return
              from dailyMeterPoints in meterPoints.GroupBy(item => item.Date)
              select new SolarViewDay
              {
                  SiteId = siteId,
                  Date = dailyMeterPoints.Key.GetSolarDateString(),
                  Meters =
                  from dailyMeterPoint in dailyMeterPoints.GroupBy(item => item.MeterType)
                  select new SolarViewMeter
                  {
                      MeterType = dailyMeterPoint.Key,
                      Points = dailyMeterPoint
                      .OrderBy(item => item.Timestamp)
                      .Select(item => new SolarViewMeterPoint
                      {
                          Timestamp = item.Timestamp,
                          Watts = item.Watts,
                          WattHour = item.WattHour
                      })
                  }
              };
        }
    }
}