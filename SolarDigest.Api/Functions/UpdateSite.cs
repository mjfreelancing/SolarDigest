using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Functions.Validators;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    /*
     
    Test payload to use in the console (lambda test):

    * Leave out any of lastAggregationDate, lastRefreshDateTime, lastSummaryDate to delete them
     
        {
            "id": "1514817",
            "site": {
                "TimeZoneId": "Australia/Sydney",
                "startDate": "2020-05-09",
                "apiKey": "api-key-here",
                "contactName": "Malcolm Smith",
                "contactEmail": "malcolm@mjfreelancing.com"
            },
            "timestamps": {
                "lastAggregationDate": "2020-05-10",
                "lastRefreshDateTime": "2020-05-11 13:00:20",
                "lastSummaryDate": "2020-05-11"
            }
        }

    */

    // For the purpose of CDK deployment, all functions need to reside in the same (SolarDigest.Api.Functions) namespace.

    public sealed class UpdateSite : FunctionBase<UpdateSitePayload, Site>
    {
        // although Site includes LastAggregationDate, LastSummaryDate, and LastRefreshDateTime, these
        // will not be returned in the response because they will not yet exist in the table.
        protected override async Task<Site> InvokeHandlerAsync(FunctionContext<UpdateSitePayload> context)
        {
            var serviceProvider = context.ScopedServiceProvider;
            var payload = context.Payload;

            serviceProvider.InvokeValidator<UpdateSitePayloadValidator, UpdateSitePayload>(payload);

            var logger = context.Logger;

            logger.LogDebug($"Creating site info for Id {payload.Id}");

            var siteTable = context.ScopedServiceProvider.GetRequiredService<ISolarDigestSiteTable>();

            var site = await siteTable!.UpsertSiteAsync(payload.Id, payload.Site, payload.Timestamps).ConfigureAwait(false);

            logger.LogDebug($"Site {site.Id} added");

            return site;
        }
    }
}