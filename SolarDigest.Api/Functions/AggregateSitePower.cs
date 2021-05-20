using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Events;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models;
using SolarDigest.Api.Payloads.EventBridge;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
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
           "endDate": "2020-05-09"
         }
       }

   */

    public sealed class AggregateSitePower : FunctionBase<AggregateSitePowerPayload, NoResult>
    {
        protected override async Task<NoResult> InvokeHandlerAsync(FunctionContext<AggregateSitePowerPayload> context)
        {
            var request = context.Payload.Detail;

            // todo: add request validation

            var logger = context.Logger;

            logger.LogDebug($"Aggregating power for site Id '{request.SiteId}' between {request.StartDate} and {request.EndDate}");

            var serviceProvider = context.ScopedServiceProvider;

            var siteTable = serviceProvider.GetService<ISolarDigestSiteTable>();

            var siteId = request.SiteId;
            var site = await siteTable!.GetItemAsync<Site>(siteId).ConfigureAwait(false);

                    // This code would normally fan out to process monthly and yearly data in parallel, but under the free AWS tier
                    // the DynamoDb throughput can become congested, so this implementation will process everything sequentially.

            // Going to try running multiple tasks


            //var siteStartDate = site.StartDate;



            var tasks = GetAggregationTasks(request, logger);

            await Task.WhenAll(tasks);

            return NoResult.Default;
        }

        private IEnumerable<Task> GetAggregationTasks(AggregateSitePowerEvent request, IFunctionLogger logger)
        {
            var startDate = request.StartDate.ParseSolarDate();
            var endDate = request.EndDate.ParseSolarDate();

            for (var year = startDate.Year; year <= endDate.Year; year++)
            {
                var aggregateStartDate = year == startDate.Year ? startDate : new DateTime(year, 1, 1);
                var aggregateEndDate = year == endDate.Year ? endDate : new DateTime(year, 12, 31);

                yield return ProcessMonthlyAggregation(aggregateStartDate, aggregateEndDate, logger);
                yield return ProcessYearlyAggregation(aggregateStartDate, aggregateEndDate, logger);
            }
        }

        private Task ProcessMonthlyAggregation(DateTime aggregateStartDate, DateTime aggregateEndDate, IFunctionLogger logger)
        {
            logger.LogDebug($"Processing monthly aggregation between {aggregateStartDate.GetSolarDateString()} and {aggregateEndDate.GetSolarDateString()}");

            return Task.CompletedTask;
        }

        private Task ProcessYearlyAggregation(DateTime aggregateStartDate, DateTime aggregateEndDate, IFunctionLogger logger)
        {
            logger.LogDebug($"Processing yearly aggregation between {aggregateStartDate.GetSolarDateString()} and {aggregateEndDate.GetSolarDateString()}");

            return Task.CompletedTask;
        }
    }
}