using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Payloads.GraphQL;
using SolarDigest.Models;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class GetSiteFunction : FunctionBase<GetSitePayload, SiteInfo>
    {
        protected override async Task<SiteInfo> InvokeHandlerAsync(FunctionContext<GetSitePayload> context)
        {
            var logger = context.Logger;

            logger.LogDebug($"Reading site info for Id '{context.Payload.Id}'");



            try
            {
                logger.LogDebug("Emulating an error");
                throw new UnauthorizedAccessException("A dummy unauthorized exception");
            }
            catch (Exception e)
            {
                var exceptionHandler = context.ScopedServiceProvider.GetService<IExceptionHandler>();
                await exceptionHandler!.HandleAsync(e);
            }




            // todo: update to read the data from DynamoDb

            var startDate = new DateTime(2020, 5, 9);
            var lastAggregationDate = DateTime.Today.Date;
            var lastSummaryDate = DateTime.Today.Date;
            var lastRefreshDateTime = DateTime.Now;

            var siteInfo = new SiteInfo
            {
                Id = "1514817",
                TimeZoneId = "AUS Eastern Standard Time",
                StartDate = $"{startDate:yyyy-MM-dd}",
                ApiKey = "OU6U7G57IBLEALXBI2XYJ3GVPB4BDJG8",
                ContactName = "Malcolm Smith",
                ContactEmail = "malcolm@mjfreelancing.com",
                LastAggregationDate = $"{lastAggregationDate:yyyy-MM-dd}",
                LastSummaryDate = $"{lastSummaryDate:yyyy-MM-dd}",
                LastRefreshDateTime = $"{lastRefreshDateTime:yyyy-MM-dd}"
            };

            return siteInfo;
        }
    }
}