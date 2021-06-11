using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Models;
using SolarDigest.Api.Processors;
using SolarDigest.Api.Repository;
using SolarDigest.Api.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    /*

   Test payload to use in the console:

   AggregateAllSitesPower determines the required date range based on the last aggregation timestamp

   To perform an update via AggregateSitePower based on an explicit date range:

       {
         "detail": {
           "siteId": "1514817",
           "startDate": "2020-05-09",
           "endDate": "2021-05-09"
         }
       }

   */

    // For the purpose of CDK deployment, all functions need to reside in the same (SolarDigest.Api.Functions) namespace.

    public sealed class AggregateSitePower : FunctionBase<AggregateSitePowerPayload, NoResult>
    {
        protected override async Task<NoResult> InvokeHandlerAsync(FunctionContext<AggregateSitePowerPayload> context)
        {
            var request = context.Payload.Detail;

            // todo: add request validation

            var logger = context.Logger;

            logger.LogDebug($"Aggregating power for site Id '{request.SiteId}' between {request.StartDate} and {request.EndDate}");

            var serviceProvider = context.ScopedServiceProvider;

            var siteTable = serviceProvider.GetRequiredService<ISolarDigestSiteTable>();

            var siteId = request.SiteId;

            // must read the entire entity because we may be updating it
            var site = await siteTable!.GetSiteAsync(siteId).ConfigureAwait(false);

            var startDate = request.StartDate.ParseSolarDate();
            var endDate = request.EndDate.ParseSolarDate();

            var lastRefreshDate = site.LastRefreshDateTime.ParseSolarDateTime().Date;

            if (startDate > lastRefreshDate)
            {
                logger.LogDebug($"The requested date range is after the latest refresh date ({lastRefreshDate.GetSolarDateString()}). Nothing to do.");
                return NoResult.Default;
            }

            if (endDate > lastRefreshDate)
            {
                logger.LogDebug($"The requested end date is after the latest refresh date ({lastRefreshDate.GetSolarDateString()}). Trimming the date range.");
                endDate = lastRefreshDate;
            }

            IEnumerable<Task> GetAggregationTasks()
            {
                for (var year = startDate.Year; year <= endDate.Year; year++)
                {
                    var aggregateStartDate = year == startDate.Year ? startDate : new DateTime(year, 1, 1);
                    var aggregateEndDate = year == endDate.Year ? endDate : new DateTime(year, 12, 31);

                    if (aggregateEndDate > endDate)
                    {
                        aggregateEndDate = endDate;
                    }

                    var monthlyProcessor = serviceProvider.GetRequiredService<IPowerMonthlyProcessor>();
                    yield return monthlyProcessor!.ProcessAsync(site, aggregateStartDate, aggregateEndDate);

                    var yearlyProcessor = serviceProvider.GetRequiredService<IPowerYearlyProcessor>();
                    yield return yearlyProcessor!.ProcessAsync(site, aggregateStartDate, aggregateEndDate);
                }
            }

            // running these (and others) all in parallel was too much for the free tier in AWS so they are being processed sequentially
            await GetAggregationTasks()
                .InvokeTasksSequentially()
                .ConfigureAwait(false);

            var siteUpdater = serviceProvider.GetRequiredService<ISiteUpdater>();
            await siteUpdater.UpdateLastAggregationDateAsync(site.Id, endDate).ConfigureAwait(false);

            return NoResult.Default;
        }
    }
}