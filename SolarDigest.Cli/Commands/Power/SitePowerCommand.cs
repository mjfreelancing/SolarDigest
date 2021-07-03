using AllOverIt.Extensions;
using AllOverIt.Helpers;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SolarDigest.Cli.Commands.Upload;
using SolarDigest.Cli.Extensions;
using SolarDigest.Models;
using System.Threading.Tasks;

namespace SolarDigest.Cli.Commands.Power
{
    // Command: power --siteId <siteId> --meterType <meterType> --summaryType <summaryType>
    //                --startDate <startDate> --endDate <endDate> --limit <limit> --startCursor <startCursor>
    // ------------------------------------------------------------------------------------------------------
    //
    // <meterType>      defaults to PRODUCTION
    // <summaryType>    defaults to DAILY_AVERAGE
    // <startDate>      defaults to site start date
    // <endDate>        defaults to the current date (in site local time)
    // <limit>          defaults to null (all data)
    // <startCursor>    defaults to null (first record)
    //
    internal sealed class SitePowerCommand : ICommand
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SitePowerCommand> _logger;

        public static string Identifier => "power";

        public SitePowerCommand(IConfiguration configuration, ILogger<SitePowerCommand> logger)
        {
            _configuration = configuration.WhenNotNull(nameof(configuration));
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task Execute()
        {
            // all via command line
            var siteId = _configuration.GetValue<string>("site");
            var meterType = _configuration.GetValue<string>("meterType");
            var summaryType = _configuration.GetValue<string>("summaryType");
            var startDate = _configuration.GetValue<string>("startDate");
            var endDate = _configuration.GetValue<string>("endDate");
            var limit = _configuration.GetValue<int?>("limit");
            var startCursor = _configuration.GetValue<string>("startCursor");

            var sitePower = await GetSitePower(siteId, meterType.As<MeterType>(), summaryType.As<SummaryType>(), startDate, endDate, limit, startCursor);





            await Task.Delay(1);
        }

        private async Task<SitePower> GetSitePower(string siteId, MeterType meterType, SummaryType summaryType, string startDate, string endDate, int? limit, string startCursor)
        {
            var graphqlUrl = _configuration.GetValue<string>("GraphqlUrl");         // via user secrets / environment variables
            var apiKey = _configuration.GetValue<string>("x-api-key");              // via user secrets / environment variables

            using (var graphQLClient = new GraphQLHttpClient(graphqlUrl, new NewtonsoftJsonSerializer()))
            {
                var request = new GraphQLHttpRequest
                {
                    Query = @"
                        query GetSite($siteId: String!, $meterType: MeterType!, $summaryType: SummaryType!, $startDate: AWSDate!, $endDate: AWSDate!, $limit: Int, $startCursor: String) {
                          site(id: $siteId) {
                            id
                            contactEmail
                            contactName
                            lastAggregationDate
                            lastRefreshDateTime
                            lastSummaryDate
                            startDate
                            timeZoneId
                            power(limit: $limit, startCursor: $startCursor, filter: {meterType: $meterType, summaryType: $summaryType, startDate: $startDate, endDate: $endDate}) {
                                  pageInfo {
                                    previousPageCursor
                                    nextPageCursor
                                  }
                                  totalCount
                                  nodes {
                                    time
                                    wattHour
                                    watts
                                  }
                                  edges {
                                    cursor
                                    node {
                                      time
                                      wattHour
                                      watts
                                    }
                                  }
                            }
                          }
                        }",
                        OperationName = "GetSite",
                        Variables = new
                        {
                            siteId,
                            meterType = $"{meterType}".ToUpperSnakeCase(),
                            summaryType = $"{summaryType}".ToUpperSnakeCase(),
                            startDate,
                            endDate,
                            limit,
                            startCursor
                        }
                };

                graphQLClient.HttpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var response = await graphQLClient.SendQueryAsync<SitePowerPayload>(request).ConfigureAwait(false);

                return response.Data.Site;
            }
        }
    }
}