using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Models;
using SolarDigest.Api.Payloads.EventBridge;
using SolarDigest.Api.Processors;
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
           "endDate": "2021-05-09"
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

            IEnumerable<Task> GetAggregationTasks()
            {
                var startDate = request.StartDate.ParseSolarDate();
                var endDate = request.EndDate.ParseSolarDate();

                for (var year = startDate.Year; year <= endDate.Year; year++)
                {
                    var aggregateStartDate = year == startDate.Year ? startDate : new DateTime(year, 1, 1);
                    var aggregateEndDate = year == endDate.Year ? endDate : new DateTime(year, 12, 31);

                    var monthlyProcessor = serviceProvider.GetService<IPowerMonthlyProcessor>();
                    yield return monthlyProcessor!.ProcessAsync(site, aggregateStartDate, aggregateEndDate);

                    var yearlyProcessor = serviceProvider.GetService<IPowerYearlyProcessor>();
                    yield return yearlyProcessor!.ProcessAsync(site, aggregateStartDate, aggregateEndDate);
                }
            }

            var tasks = GetAggregationTasks();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return NoResult.Default;
        }
    }
}