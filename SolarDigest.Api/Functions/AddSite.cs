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
     
        {
            "id": "1514817",
            "site": {
                "TimeZoneId": "Australia/Sydney",
                "startDate": "2020-05-09",
                "apiKey": "api-key-here",
                "contactName": "Malcolm Smith",
                "contactEmail": "malcolm@mjfreelancing.com"
            }
        }

    */

    // For the purpose of CDK deployment, all functions need to reside in the same (SolarDigest.Api.Functions) namespace.

    public sealed class AddSite : FunctionBase<AddSitePayload, Site>
    {
        // although Site includes LastAggregationDate, LastSummaryDate, and LastRefreshDateTime, these
        // will not be returned in the response because they will not yet exist in the table.
        protected override async Task<Site> InvokeHandlerAsync(FunctionContext<AddSitePayload> context)
        {
            var serviceProvider = context.ScopedServiceProvider;
            var payload = context.Payload;

            serviceProvider.InvokeValidator<AddSitePayloadValidator, AddSitePayload>(payload);

            var logger = context.Logger;

            logger.LogDebug($"Creating site info for Id {payload.Id}");

            var siteTable = serviceProvider.GetRequiredService<ISolarDigestSiteTable>();

            var site = await siteTable!.AddSiteAsync(payload.Id, payload.Site);

            logger.LogDebug($"Site {payload.Id} added");

            return site;
        }
    }
}