using AllOverIt.Extensions;
using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Functions.Validators;
using SolarDigest.Api.Models;
using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Summarizers;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{

    /*

    query GetSite($siteId: String!, $meterType: MeterType!, $summaryType: SummaryType!, $startDate: AWSDate!, $endDate: AWSDate!) {
      site(id: $siteId) {
        id
        contactEmail
        contactName
        lastAggregationDate
        lastRefreshDateTime
        lastSummaryDate
        startDate
        timeZoneId
        power(filter: {meterType: $meterType, summaryType: $summaryType, startDate: $startDate, endDate: $endDate}) {
          nextToken
          timeWatts {
            time
            wattHour
            watts
          }
        }
      }
    }

    with variables:

    {
        "siteId": "1514817",
        "meterType": "PRODUCTION",
        "summaryType": "AVERAGE",
        "startDate": "2021-06-01",
        "endDate": "2021-06-07"
    }

    */

    // For the purpose of CDK deployment, all functions need to reside in the same (SolarDigest.Api.Functions) namespace.

    public sealed class GetSitePowerSummary : FunctionBase<GetSitePowerSummaryPayload, SitePower>
    {
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

            var timeWatts = summaryType switch
            {
                SummaryType.Average => await GetDailyAveragePowerSummary(serviceProvider, siteId, meterType, startDate, endDate).ConfigureAwait(false),
                _ => throw new InvalidOperationException($"Unexpected summary type '{summaryType}'")
            };

            logger.LogDebug("Returning with the requested power summary");

            return new SitePower(timeWatts);
        }

        private static Task<IEnumerable<TimeWatts>> GetDailyAveragePowerSummary(IServiceProvider serviceProvider, string siteId, MeterType meterType,
            DateTime startDate, DateTime endDate)
        {
            var summarizer = serviceProvider.GetRequiredService<IDailyAveragePowerSummarizer>();

            return summarizer.GetTimeWattsAsync(siteId, meterType, startDate, endDate);
        }
    }
}