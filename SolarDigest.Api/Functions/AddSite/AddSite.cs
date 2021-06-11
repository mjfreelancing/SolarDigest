using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Repository;
using SolarDigest.Api.Validation;
using SolarDigest.Api.Validation.Extensions;
using SolarDigest.Models;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions.AddSite
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

    public sealed class AddSite : FunctionBase<AddSitePayload, ISite>
    {
        // although Site includes LastAggregationDate, LastSummaryDate, and LastRefreshDateTime, these
        // will not be returned in the response because they will not yet exist in the table.
        protected override async Task<ISite> InvokeHandlerAsync(FunctionContext<AddSitePayload> context)
        {
            var logger = context.Logger;
            var serviceProvider = context.ScopedServiceProvider;
            var payload = context.Payload;

            serviceProvider.InvokeValidator<AddSitePayloadValidator, AddSitePayload>(payload);

            logger.LogDebug($"Creating site info for Id '{payload.Id}'");

            var siteTable = serviceProvider.GetRequiredService<ISolarDigestSiteTable>();

            var site = await siteTable!.AddSiteAsync(payload.Id, payload.Site);

            logger.LogDebug($"Site '{payload.Id}' added");

            return site;
        }
    }
}