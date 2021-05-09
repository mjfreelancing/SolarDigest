using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Payloads.GraphQL;
using SolarDigest.Api.Services;
using SolarDigest.Models;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class AddSite : FunctionBase<AddSitePayload, Site>
    {
        protected override Task<Site> InvokeHandlerAsync(FunctionContext<AddSitePayload> context)
        {
            var logger = context.Logger;

            var payload = context.Payload;

            logger.LogDebug($"Creating site info for Id '{payload.Id}'");

            var dynamoDb = context.ScopedServiceProvider.GetService<ISolarDigestDynamoDb>();
            var table = dynamoDb!.GetTable("Site");

            var doc = new Document();


            //doc["id"] = payload.Id;



            // todo: update to read the data from DynamoDb

            var startDate = new DateTime(2020, 5, 9);
            var lastAggregationDate = DateTime.Today.Date;
            var lastSummaryDate = DateTime.Today.Date;
            var lastRefreshDateTime = DateTime.Now;

            var site = new Site
            {
                Id = "1514817",
                TimeZoneId = "AUS Eastern Standard Time",
                StartDate = $"{startDate:yyyy-MM-dd}",
                ApiKey = "XYZ",
                ContactName = "Malcolm Smith",
                ContactEmail = "malcolm@mjfreelancing.com",
                LastAggregationDate = $"{lastAggregationDate:yyyy-MM-dd}",
                LastSummaryDate = $"{lastSummaryDate:yyyy-MM-dd}",
                LastRefreshDateTime = $"{lastRefreshDateTime:yyyy-MM-dd}"
            };

            return Task.FromResult(site);
        }
    }
}