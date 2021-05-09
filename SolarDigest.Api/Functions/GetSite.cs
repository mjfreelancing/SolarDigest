using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Payloads.GraphQL;
using SolarDigest.Api.Services;
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
      addSite(id: "df", site: {apiKey: "cvbcvb", contactEmail: "", contactName: "", lastAggregationDate: "", lastRefreshDateTime: "", lastSummaryDate: "", startDate: "", timeZoneId: ""}) {
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

            var db = context.ScopedServiceProvider.GetService<ISolarDigestDynamoDb>();

            // await here in case there is an exception
            return await db!.GetItemAsync<Site>(Api.Constants.Table.Site, payload.Id);
            


            //var startDate = new DateTime(2020, 5, 9);
            //var lastAggregationDate = DateTime.Today.Date;
            //var lastSummaryDate = DateTime.Today.Date;
            //var lastRefreshDateTime = DateTime.Now;

            //var site = new Site
            //{
            //    Id = "1514817",
            //    TimeZoneId = "AUS Eastern Standard Time",
            //    StartDate = $"{startDate:yyyy-MM-dd}",
            //    ApiKey = "XYZ",
            //    ContactName = "Malcolm Smith",
            //    ContactEmail = "malcolm@mjfreelancing.com",
            //    LastAggregationDate = $"{lastAggregationDate:yyyy-MM-dd}",
            //    LastSummaryDate = $"{lastSummaryDate:yyyy-MM-dd}",
            //    LastRefreshDateTime = $"{lastRefreshDateTime:yyyy-MM-dd}"
            //};

            //return Task.FromResult(site);
        }
    }
}