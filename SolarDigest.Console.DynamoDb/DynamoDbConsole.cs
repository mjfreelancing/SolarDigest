using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using ConsoleService;
using Microsoft.Extensions.Configuration;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SolarDigest.Console.DynamoDb
{
    public class DynamoDbConsole : IConsoleApp
    {
        private readonly IConfiguration _configuration;

        public DynamoDbConsole(IConfiguration configuration)
        {
            _configuration = configuration.WhenNotNull(nameof(configuration));
        }

        public Task<int> Execute()
        {
            return CreateSiteIfMissing(
                _configuration["SolarEdgeApiKey"],
                _configuration["AwsAccessKey"],
                _configuration["AwsSecretKey"]
                );
        }


        // todo: need to look at
        //  - https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DotNetSDKMidLevel.html
        //  - https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DotNetSDKHighLevel.html


        private static async Task<int> CreateSiteIfMissing(string solarEdgeApiKey, string awsAccessKey, string awsSecretKey)
        {
            var credentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
            var dbClient = new AmazonDynamoDBClient(credentials);

            try
            {
                var request = new QueryRequest
                {
                    TableName = "Site",
                    KeyConditionExpression = "Id = :id",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {
                            ":id",
                            new AttributeValue
                            {
                                S = "1514817"
                            }
                        }
                    }
                };

                var response = await dbClient.QueryAsync(request);

                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    var item = response.Items.SingleOrDefault();

                    if (item == null)
                    {
                        var startDate = new DateTime(2020, 5, 9);
                        var lastAggregationDate = DateTime.Today.Date;
                        var lastSummaryDate = DateTime.Today.Date;
                        var lastRefreshDateTime = DateTime.Now;

                        var site = new Site
                        {
                            Id = "1514817",
                            TimeZoneId = "AUS Eastern Standard Time",
                            StartDate = $"{startDate:yyyy-MM-dd}",
                            ApiKey = solarEdgeApiKey,
                            ContactName = "Malcolm Smith",
                            ContactEmail = "malcolm@mjfreelancing.com",
                            LastAggregationDate = $"{lastAggregationDate:yyyy-MM-dd}",
                            LastSummaryDate = $"{lastSummaryDate:yyyy-MM-dd}",
                            LastRefreshDateTime = $"{lastRefreshDateTime:yyyy-MM-dd}"
                        };

                        var siteValues = site.ToPropertyDictionary();

                        var attributeValues = siteValues.ToDictionary(
                            kvp => kvp.Key,
                            kvp => new AttributeValue {S = $"{kvp.Value}"});

                        var putItem = new PutItemRequest
                        {
                            TableName = "Site",
                            Item = attributeValues
                        };

                        await dbClient.PutItemAsync(putItem);
                    }
                }
                else
                {
                    // throw
                }
            }
            catch (AmazonDynamoDBException exception)
            {
                System.Console.WriteLine(exception);
                return -2;
            }
            catch (Exception exception)
            {
                System.Console.WriteLine(exception);
                return -1;
            }

            return 0;
        }
    }
}