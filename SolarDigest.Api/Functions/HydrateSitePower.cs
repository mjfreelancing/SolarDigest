using AllOverIt.Extensions;
using AllOverIt.Tasks;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SolarDigest.Api.Data;
using SolarDigest.Api.Events;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models;
using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Models.SolarEdgeData;
using SolarDigest.Api.Payloads.EventBridge;
using SolarDigest.Api.Repository;
using SolarDigest.Api.Services.SolarEdge;
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
            "startDateTime": "2020-05-09 00:00:00",
            "endDateTime": "2020-05-09 01:00:00"
          }
        }

    */

    public sealed class HydrateSitePower : FunctionBase<HydrateSitePowerPayload, NoResult>
    {
        // The solarEdge API can process, at most, 1 month of data at a time but we are limiting requests
        // to 7 days in order to restrict the execution time of the function and the throughput to DynamoDb.
        private const int MaxDaysToProcess = 7;

        protected override async Task<NoResult> InvokeHandlerAsync(FunctionContext<HydrateSitePowerPayload> context)
        {
            var request = context.Payload.Detail;
            var siteId = request.SiteId;

            // todo: add request validation

            var logger = context.Logger;

            logger.LogDebug($"Hydrating power for site Id '{siteId}'");

            var serviceProvider = context.ScopedServiceProvider;

            // get site info
            var siteTable = serviceProvider.GetService<ISolarDigestSiteTable>();
            var site = await siteTable!.GetItemAsync<Site>(siteId).ConfigureAwait(false);

            // determine the refresh period to hydrate (local time)
            // - the start/end date/time are optional - a forced re-fresh can be achieved when providing them
            var (hydrateStartDateTime, hydrateEndDateTime) = GetHydrationPeriodInSiteLocalTime(site, request);

            // see if we need to process in smaller time periods
            if (hydrateEndDateTime - hydrateStartDateTime > TimeSpan.FromDays(MaxDaysToProcess))
            {
                // todo: add this - some other status to indicate the request is being made more granular
                // await NotifyPowerUpdated(context, PowerUpdatedStatus.Started, triggeredPowerQuery);

                await SendEventsToProcessWeeklyAsync(siteId, hydrateStartDateTime, hydrateEndDateTime, logger).ConfigureAwait(false);
                return NoResult.Default;
            }

            logger.LogDebug($"Site '{siteId}' will be hydrated for the period {hydrateStartDateTime.GetSolarDateTimeString()} to " +
                            $"{hydrateEndDateTime.GetSolarDateTimeString()} (site local time)");


            // todo: add this
            // await NotifyPowerUpdated(context, PowerUpdatedStatus.Started, triggeredPowerQuery);


            var solarEdgeApi = serviceProvider.GetService<ISolarEdgeApi>();
            var mapper = serviceProvider.GetService<IMapper>();
            var repository = serviceProvider.GetService<ISolarDigestPowerTable>();

            await ProcessPowerForDateRange(siteId, solarEdgeApi, repository, hydrateStartDateTime, hydrateEndDateTime, mapper, logger).ConfigureAwait(false);


            // todo: add this
            // NotifyPowerUpdated


            return NoResult.Default;
        }

        private static async Task ProcessPowerForDateRange(string siteId, ISolarEdgeApi solarEdgeApi, ISolarDigestPowerTable repository,
            DateTime hydrateStartDateTime, DateTime hydrateEndDateTime, IMapper mapper, IFunctionLogger logger)
        {
            var startDateTime = hydrateStartDateTime.GetSolarDateTimeString();      // in site local time
            var endDateTime = hydrateEndDateTime.GetSolarDateTimeString();

            logger.LogDebug($"Processing period: {startDateTime} to {endDateTime}");

            var powerQuery = new PowerQuery
            {
                SiteId = siteId,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime
            };

            var (powerResults, energyResults) = await TaskHelper.WhenAll(
                solarEdgeApi!.GetPowerDetailsAsync(powerQuery),
                solarEdgeApi!.GetEnergyDetailsAsync(powerQuery)
            ).ConfigureAwait(false);

            var powerData = mapper!.Map<SolarData>(powerResults);
            var energyData = mapper.Map<SolarData>(energyResults);

            var solarViewDays = GetSolarViewDays(powerQuery.SiteId, powerData, energyData);

            foreach (var solarViewDay in solarViewDays)
            {
                var entities = solarViewDay.Meters
                    .SelectMany(
                        meter => meter.Points,
                        (meter, point) => new MeterPowerEntity(solarViewDay.SiteId, point.Timestamp, meter.MeterType, point.Watts, point.WattHour))
                    .AsReadOnlyList();

                await repository.PutItemsAsync(entities).ConfigureAwait(false);
            }
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

        private static (DateTime, DateTime) GetHydrationPeriodInSiteLocalTime(Site site, HydrateSitePowerEvent request)
        {
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

            return (hydrateStartDateTime, hydrateEndDateTime);
        }

        private static async Task SendEventsToProcessWeeklyAsync(string siteId, DateTime hydrateStartDateTime, DateTime hydrateEndDateTime, IFunctionLogger logger)
        {
            var requestStartDate = hydrateStartDateTime.ToUniversalTime();
            var requestEndDate = hydrateEndDateTime.ToUniversalTime();

            if (requestEndDate - requestStartDate > TimeSpan.FromDays(7))
            {
                var client = new AmazonEventBridgeClient();

                var weeklyPeriods = requestStartDate
                    .GetWeeklyDateRangesUntil(requestEndDate)
                    .AsReadOnlyCollection();

                logger.LogDebug($"Posting events to handle the range {hydrateStartDateTime.GetSolarDateTimeString()} to {hydrateEndDateTime.GetSolarDateTimeString()} as " +
                                $"{weeklyPeriods.Count} weekly periods");

                var putBatches = weeklyPeriods.Batch(4);

                foreach (var batch in putBatches)
                {
                    var entries = batch
                        .Select(item => new PutEventsRequestEntry
                        {
                            Source = Constants.Events.Source,
                            EventBusName = "default",
                            DetailType = nameof(HydrateSitePowerEvent),
                            Time = DateTime.Now,
                            Detail = JsonConvert.SerializeObject(new HydrateSitePowerEvent
                            {
                                SiteId = siteId,
                                StartDateTime = $"{item.StartDateTime.GetSolarDateTimeString()}",
                                EndDateTime = $"{item.EndDateTime.GetSolarDateTimeString()}"
                            })
                        })
                        .ToList();

                    var putRequest = new PutEventsRequest
                    {
                        Entries = entries
                    };

                    await client.PutEventsAsync(putRequest).ConfigureAwait(false);
                }
            }
        }
    }
}