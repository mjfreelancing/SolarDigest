﻿using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SolarDigest.Api.Events;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Payloads.EventBridge;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class AggregateAllSitesPower : FunctionBase<AggregateAllSitesPowerPayload, bool>
    {
        protected override async Task<bool> InvokeHandlerAsync(FunctionContext<AggregateAllSitesPowerPayload> context)
        {
            var logger = context.Logger;
            logger.LogDebug("Aggregating power for all sites");

            AmazonEventBridgeClient client = null;

            var currentTimeUtc = DateTime.UtcNow;
            var siteTable = context.ScopedServiceProvider.GetService<ISolarDigestSiteTable>();

            // only retrieve the properties we need
            var sites = siteTable!.ScanAsync<Site>(null, new[]
            {
                nameof(Site.Id), nameof(Site.TimeZoneId), nameof(Site.StartDate), nameof(Site.LastAggregationDate)
            });

            await foreach (var site in sites)
            {
                logger.LogDebug($"Converting {currentTimeUtc.GetSolarDateTimeString()} to site {site.Id} local time ({site.TimeZoneId})");

                var siteLocalTime = site.UtcToLocalTime(currentTimeUtc);

                // check subsequent hours in case a trigger was missed
                if (siteLocalTime.Hour >= Constants.RefreshHour.Aggregation)
                {
                    var lastAggregationDate = site.GetLastAggregationDate();
                    var nextEndDate = siteLocalTime.Date.AddDays(-1);         // not reporting the current day as it is not yet over

                    if (nextEndDate > lastAggregationDate)
                    {
                        client ??= new AmazonEventBridgeClient();

                        var aggregateEvent = new AggregateSitePowerEvent
                        {
                            SiteId = site.Id,
                            StartDate = lastAggregationDate.GetSolarDateString(),
                            EndDate = nextEndDate.GetSolarDateString(),
                        };

                        logger.LogDebug($"Sending an aggregation request for site {site.Id} between {aggregateEvent.StartDate} and {aggregateEvent.EndDate} (local)");

                        var putRequest = new PutEventsRequest
                        {
                            Entries = new List<PutEventsRequestEntry>
                            {
                                new()
                                {
                                    Source = Constants.Events.Source,
                                    EventBusName = "default",
                                    DetailType = nameof(AggregateSitePowerEvent),
                                    Time = DateTime.Now,
                                    Detail = JsonConvert.SerializeObject(aggregateEvent)
                                }
                            }
                        };

                        await client.PutEventsAsync(putRequest).ConfigureAwait(false);
                    }
                }
            }

            logger.LogDebug("All Sites have been iterated");

            return true;
        }
    }
}