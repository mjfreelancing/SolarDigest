using AllOverIt.Helpers;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using SolarDigest.Graphql.Extensions;
using SolarDigest.Graphql.Responses;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Graphql
{
    public sealed class SolarDigestGraphql : ISolarDigestGraphql
    {
        private readonly ISolarDigestGraphqlConfiguration _configuration;

        public SolarDigestGraphql(ISolarDigestGraphqlConfiguration configuration)
        {
            _configuration = configuration.WhenNotNull(nameof(configuration));
        }

        public async Task<GetSiteResponse> GetSiteAsync(string siteId)
        {
            var query = @"
                        query GetSite($siteId: String!) {
                          site(id: $siteId) {
                            id
                            contactEmail
                            contactName
                            lastAggregationDate
                            lastRefreshDateTime
                            lastSummaryDate
                            startDate
                            timeZoneId                           
                          }
                        }";

            var operationName = "GetSite";
            var variables = new { siteId };

            return await QueryAsync<SitePayload, GetSiteResponse>(query, operationName, variables, response => response.Site).ConfigureAwait(false);
        }

        public async Task<GetSitePowerResponse> GetSitePowerAsync(string siteId, string meterType, string summaryType, string startDate, string endDate, int? limit, string startCursor)
        {
            var query = @"
                        query GetSitePower($siteId: String!, $meterType: MeterType!, $summaryType: SummaryType!, $startDate: AWSDate!, $endDate: AWSDate!, $limit: Int, $startCursor: String) {
                          site(id: $siteId) {
                            power(limit: $limit, startCursor: $startCursor, filter: {meterType: $meterType, summaryType: $summaryType, startDate: $startDate, endDate: $endDate}) {
                              pageInfo {
                                previousPageCursor
                                nextPageCursor
                              }
                              totalCount
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
                        }";

            var operationName = "GetSitePower";
            var variables = new
            {
                siteId,
                meterType = $"{meterType}".ToUpperSnakeCase(),
                summaryType = $"{summaryType}".ToUpperSnakeCase(),
                startDate,
                endDate,
                limit,
                startCursor
            };

            var sitePower = await QueryAsync<SitePowerPayload, PowerPayload>(query, operationName, variables, response => response.Site).ConfigureAwait(false);

            return sitePower.Power;
        }

        private async Task<TResult> QueryAsync<TResponse, TResult>(string query, string operationName, object variables, Func<TResponse, TResult> resultResolver)
        {
            using (var client = new GraphQLHttpClient(_configuration.ApiUrl, new NewtonsoftJsonSerializer()))
            {
                var request = new GraphQLHttpRequest
                {
                    Query = query,
                    OperationName = operationName,
                    Variables = variables
                };

                client.HttpClient.DefaultRequestHeaders.Add("x-api-key", _configuration.ApiKey);

                var response = await client.SendQueryAsync<TResponse>(request).ConfigureAwait(false);

                return resultResolver.Invoke(response.Data);
            }
        }
    }
}