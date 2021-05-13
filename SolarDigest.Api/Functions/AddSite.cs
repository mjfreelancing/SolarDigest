using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Payloads.GraphQL;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    /*
     
    Test payload to use in the console:
     
        {
            "id": "1514817",
            "site": {
                "TimeZoneId": "Australia/Sydney",
                "startDate": "2020-05-09",
                "apiKey": "api-key-here",
                "contactName": "Malcolm Smith",
                "contactEmail": "malcolm@mjfreelancing.com",
                "lastAggregationDate": "",
                "lastSummaryDate": "",
                "lastRefreshDateTime": ""
            }
        }

    */

    public sealed class AddSite : FunctionBase<AddSitePayload, Site>
    {
        protected override async Task<Site> InvokeHandlerAsync(FunctionContext<AddSitePayload> context)
        {
            var logger = context.Logger;

            var payload = context.Payload;

            logger.LogDebug($"Creating site info for Id '{payload.Id}'");

            var site = payload.Site;

            var entity = new Site
            {
                Id = payload.Id,
                TimeZoneId = site.TimeZoneId, // "AUS Eastern Standard Time" or "Australia/Sydney",
                StartDate = site.StartDate,
                ApiKey = site.ApiKey,
                ContactName = site.ContactName,
                ContactEmail = site.ContactEmail,
                LastAggregationDate = site.LastAggregationDate,
                LastSummaryDate = site.LastSummaryDate,
                LastRefreshDateTime = site.LastRefreshDateTime
            };

            var siteTable = context.ScopedServiceProvider.GetService<ISolarDigestSiteTable>();

            await siteTable!.AddItemAsync(entity);

            logger.LogDebug($"Site '{payload.Id}' added");

            return entity;
        }
    }
}