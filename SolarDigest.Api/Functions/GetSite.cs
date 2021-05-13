using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Payloads.GraphQL;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    /*

     query MyQuery {
      site(id: "1514817") {
        id
        contactEmail
        contactName
        lastAggregationDate
        lastRefreshDateTime
        lastSummaryDate
        startDate
        timeZoneId
      }
    }


    mutation MyMutation {
      addSite(id: "1514817", site: {apiKey: "api-key-here", contactEmail: "malcolm@mjfreelancing.com", contactName: "Malcolm Smith", lastAggregationDate: "2020-05-09", lastRefreshDateTime: "2020-05-09", lastSummaryDate: "2020-05-09", startDate: "2020-05-09", timeZoneId: "AUS Eastern Standard Time"}) {
        id
        contactEmail
        contactName
      }
    }

    */

    public sealed class GetSite : FunctionBase<GetSitePayload, Site>
    {
        protected override async Task<Site> InvokeHandlerAsync(FunctionContext<GetSitePayload> context)
        {
            var logger = context.Logger;
            var payload = context.Payload;

            logger.LogDebug($"Reading site info for Id '{payload.Id}'");

            var siteTable = context.ScopedServiceProvider.GetService<ISolarDigestSiteTable>();

            // await here in case there is an exception
            return await siteTable!.GetItemAsync<Site>(payload.Id);
        }
    }
}