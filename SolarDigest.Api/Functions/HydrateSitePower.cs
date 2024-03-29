﻿using AllOverIt.Extensions;
using AllOverIt.Tasks;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SolarDigest.Api.Events;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models;
using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Models.SolarEdgeData;
using SolarDigest.Api.Repository;
using SolarDigest.Api.Services;
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

    // For the purpose of CDK deployment, all functions need to reside in the same (SolarDigest.Api.Functions) namespace.

    public sealed class HydrateSitePower : FunctionBase<HydrateSitePowerPayload, NoResult>
    {
        // The solarEdge API can process, at most, 1 month of data at a time but we are limiting requests
        // to 7 days in order to restrict the execution time of the function and the throughput to DynamoDb
        // (using the free tier).
        private const int MaxDaysPerInvocation = 7;
        private const int MaxDaysPerRequest = MaxDaysPerInvocation * 16;        // ensures completion well within an hour

        protected override async Task<NoResult> InvokeHandlerAsync(FunctionContext<HydrateSitePowerPayload> context)
        {
            var request = context.Payload.Detail;
            var siteId = request.SiteId;

            // todo: add request validation

            var logger = context.Logger;

            logger.LogDebug($"Hydrating power for site Id {siteId}");

            var serviceProvider = context.ScopedServiceProvider;

            var siteTable = serviceProvider.GetRequiredService<ISolarDigestSiteTable>();

            // must read the entire entity because we may be updating it
            var site = await siteTable!.GetSiteAsync(siteId).ConfigureAwait(false);

            // Determine the refresh period to hydrate (local time)
            // - the start/end timestamps are optional - a forced re-fresh can be achieved when providing them
            var (hydrateStartDateTime, hydrateEndDateTime) = GetHydrationPeriodInSiteLocalTime(site, request);

            // If the last refresh timestamp is set to a future date (in the database) then ignore the request.
            // Useful when wanting to temporarily disable processing (while testing new code).
            if (hydrateStartDateTime > hydrateEndDateTime)
            {
                return NoResult.Default;
            }

            // Normally this is where the code would fan out to multiple requests if the date period was extensive.
            // Under the free AWS plan, though, testing showed that multiple instances of the function running only
            // led to excessive throughput exceptions. While these could be throttled this only means there's a potential
            // for multiple functions consuming CPU time while doing nothing.
            // So, in this version of the code, the function will process 1 week of data, at most, and then post an
            // event if there is more data to be processed. This function will exit and another will pick up the next
            // message. This approach then means the throughput won't be as bad and we can more easily track the most
            // recent refresh timestamp.

            // Having said that, we will process at-most 3 months for any given request. A full year of data, as an example,
            // can take 2 hours to complete due to throttling and since hydration occurs every hour we are limiting the amount
            // of data that can be processed per request.
            if (hydrateEndDateTime - hydrateStartDateTime > TimeSpan.FromDays(MaxDaysPerRequest))
            {
                hydrateEndDateTime = hydrateStartDateTime.AddDays(MaxDaysPerRequest - 1);

                logger.LogDebug($"Limiting the request to a maximum of {MaxDaysPerRequest} days: " +
                                $"{hydrateStartDateTime.GetSolarDateTimeString()} to {hydrateEndDateTime.GetSolarDateTimeString()}");
            }

            var maxAllowedEndDate = hydrateStartDateTime.AddDays(MaxDaysPerInvocation);

            var processingToEndDate = hydrateEndDateTime > maxAllowedEndDate
                ? maxAllowedEndDate
                : hydrateEndDateTime;

            logger.LogDebug($"Site {siteId} being hydrated for the period {hydrateStartDateTime.GetSolarDateTimeString()} to " +
                            $"{processingToEndDate.GetSolarDateTimeString()} (site local time)");

            var updateHistoryTable = serviceProvider.GetRequiredService<ISolarDigestPowerUpdateHistoryTable>();

            Task UpdatePowerHistoryAsync(PowerUpdateStatus status)
            {
                logger.LogDebug($"Updating power history for site {siteId} as '{status}' ({hydrateStartDateTime.GetSolarDateTimeString()} to {processingToEndDate.GetSolarDateTimeString()})");

                return updateHistoryTable!.UpsertPowerStatusHistoryAsync(siteId, hydrateStartDateTime, processingToEndDate, status);
            }

            await UpdatePowerHistoryAsync(PowerUpdateStatus.Started).ConfigureAwait(false);

            try
            {
                var solarEdgeApi = serviceProvider.GetRequiredService<ISolarEdgeApi>();
                var powerTable = serviceProvider.GetRequiredService<ISolarDigestPowerTable>();
                var mapper = serviceProvider.GetRequiredService<IMapper>();

                await ProcessPowerForDateRangeAsync(site, solarEdgeApi, powerTable, hydrateStartDateTime, processingToEndDate, mapper, logger).ConfigureAwait(false);
                await UpdatePowerHistoryAsync(PowerUpdateStatus.Completed).ConfigureAwait(false);

                var siteUpdater = serviceProvider.GetRequiredService<ISiteUpdater>();
                await siteUpdater.UpdateLastRefreshDateTimeAsync(site.Id, processingToEndDate).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // This will result in any pending dates to be aborted, but they will be processed the next time there
                // is a scheduled trigger because the refresh timestamp will have not been updated.
                await UpdatePowerHistoryAsync(PowerUpdateStatus.Error).ConfigureAwait(false);

                throw;
            }
            
            if (processingToEndDate != hydrateEndDateTime)
            {
                await SendEventToProcessRemainingPeriodAsync(siteId, processingToEndDate, hydrateEndDateTime, logger);
            }

            return NoResult.Default;
        }

        private static async Task ProcessPowerForDateRangeAsync(Site site, ISolarEdgeApi solarEdgeApi, ISolarDigestPowerTable powerTable,
            DateTime hydrateStartDateTime, DateTime hydrateEndDateTime, IMapper mapper, ISolarDigestLogger logger)
        {
            var startDateTime = hydrateStartDateTime.GetSolarDateTimeString();      // In site local time
            var endDateTime = hydrateEndDateTime.GetSolarDateTimeString();

            logger.LogDebug($"Processing period: {startDateTime} to {endDateTime}");

            var powerQuery = new PowerQuery
            {
                SiteId = site.Id,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime
            };

            var (powerResults, energyResults) = await TaskHelper.WhenAll(
                solarEdgeApi.GetPowerDetailsAsync(site.ApiKey, powerQuery),
                solarEdgeApi.GetEnergyDetailsAsync(site.ApiKey, powerQuery)
            ).ConfigureAwait(false);

            var powerData = mapper!.Map<SolarData>(powerResults);
            var energyData = mapper.Map<SolarData>(energyResults);

            var solarViewDays = GetSolarViewDays(powerQuery.SiteId, powerData, energyData);

            foreach (var solarViewDay in solarViewDays)
            {
                logger.LogDebug($"Persisting data for {solarViewDay.Date}");

                await powerTable.AddMeterPowerAsync(solarViewDay).ConfigureAwait(false);
            }
        }

        private static IEnumerable<SolarViewDay> GetSolarViewDays(string siteId, SolarData powerData, SolarData energyData)
        {
            // Flattened list of data points - so we can group into days and meter types.
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
                                  energy.WattHour
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

        private static (DateTime, DateTime) GetHydrationPeriodInSiteLocalTime(ISite site, HydrateSitePowerEvent request)
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

        private static async Task SendEventToProcessRemainingPeriodAsync(string siteId, DateTime startDateTime, DateTime endDateTime,
            ISolarDigestLogger logger)
        {
            logger.LogDebug($"Posting an event to handle the range {startDateTime.GetSolarDateTimeString()} to {endDateTime.GetSolarDateTimeString()}");

            var client = new AmazonEventBridgeClient();

            var putRequest = new PutEventsRequest
            {
                Entries = new List<PutEventsRequestEntry>
                {
                    new()
                    {
                        Source = Shared.Constants.Events.Source,
                        EventBusName = "default",
                        DetailType = nameof(HydrateSitePowerEvent),
                        Time = DateTime.Now,
                        Detail = JsonConvert.SerializeObject(new HydrateSitePowerEvent
                        {
                            SiteId = siteId,
                            StartDateTime = $"{startDateTime.GetSolarDateTimeString()}",
                            EndDateTime = $"{endDateTime.GetSolarDateTimeString()}"
                        })
                    }
                }
            };

            await client.PutEventsAsync(putRequest).ConfigureAwait(false);
        }
    }
}