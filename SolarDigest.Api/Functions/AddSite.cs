using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Payloads.GraphQL;
using SolarDigest.Api.Services;
using SolarDigest.Models;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
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
                TimeZoneId = site.TimeZoneId, //"AUS Eastern Standard Time",
                StartDate = site.StartDate,   //$"{startDate:yyyy-MM-dd}",
                ApiKey = site.ApiKey,
                ContactName = site.ContactName,
                ContactEmail = site.ContactEmail,
                LastAggregationDate = site.LastAggregationDate,
                LastSummaryDate = site.LastSummaryDate,
                LastRefreshDateTime = site.LastRefreshDateTime
            };

            var db = context.ScopedServiceProvider.GetService<ISolarDigestDynamoDb>();

            await db!.PutItemAsync(Api.Constants.Table.Site, entity);

            logger.LogDebug($"Site '{payload.Id}' added");

            return entity;
        }
    }
}