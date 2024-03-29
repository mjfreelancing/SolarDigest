﻿using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SolarDigest.Api.Events;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    // For the purpose of CDK deployment, all functions need to reside in the same (SolarDigest.Api.Functions) namespace.

    public sealed class AggregateAllSitesPower : FunctionBase<AggregateAllSitesPowerPayload, bool>
    {
        protected override async Task<bool> InvokeHandlerAsync(FunctionContext<AggregateAllSitesPowerPayload> context)
        {
            var logger = context.Logger;
            logger.LogDebug("Processing power aggregation for all sites");

            AmazonEventBridgeClient client = null;

            var currentTimeUtc = DateTime.UtcNow;
            var siteTable = context.ScopedServiceProvider.GetRequiredService<ISolarDigestSiteTable>();

            // only retrieve the properties we need
            var sites = siteTable!.GetAllSitesAsync(new[]
            {
                nameof(Site.Id), nameof(Site.TimeZoneId), nameof(Site.StartDate), nameof(Site.LastRefreshDateTime), nameof(Site.LastAggregationDate)
            });

            await foreach (var site in sites)
            {
                var siteLocalTime = site.UtcToLocalTime(currentTimeUtc);

                logger.LogDebug($"Converted {currentTimeUtc.GetSolarDateTimeString()} to site {site.Id} local time " +
                                $"{siteLocalTime.GetSolarDateTimeString()} ({site.TimeZoneId})");

                // check subsequent hours in case a trigger was missed
                if (siteLocalTime.Hour >= Constants.RefreshHour.SitePowerAggregation)
                {
                    // make sure we don't aggregate beyond the last refresh timestamp
                    var lastRefreshTimestamp = site.GetLastRefreshDateTime();

                    if (lastRefreshTimestamp == DateTime.MinValue)
                    {
                        // must be the first time, so assume the site start date
                        lastRefreshTimestamp = site.StartDate.ParseSolarDate().Date;
                    }

                    var lastAggregationDate = site.GetLastAggregationDate();

                    if (lastAggregationDate == DateTime.MinValue)
                    {
                        // must be the first time, so assume the site start date
                        lastAggregationDate = site.StartDate.ParseSolarDate().Date;
                    }

                    var nextEndDate = siteLocalTime.Date.AddDays(-1);         // not reporting the current day as it is not yet over

                    if (nextEndDate > lastRefreshTimestamp)
                    {
                        logger.LogDebug($"Trimming the next aggregation end date to match the last refresh date {lastRefreshTimestamp.GetSolarDateString()}");
                        nextEndDate = lastRefreshTimestamp;
                    }

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
                                    Source = Shared.Constants.Events.Source,
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