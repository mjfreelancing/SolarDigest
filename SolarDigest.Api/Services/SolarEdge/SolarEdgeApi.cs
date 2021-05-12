using AllOverIt.Helpers;
using AllOverIt.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Flurl;
using Flurl.Http;
using SolarDigest.Api.Exceptions;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Services.SolarEdge.Response;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services.SolarEdge
{
    internal sealed class SolarEdgeApi : ISolarEdgeApi
    {
        private readonly AsyncLazy<string> _apiKey = new(GetApiKeyAsync);
        private readonly IFunctionLogger _logger;

        public SolarEdgeApi(IFunctionLogger logger)
        {
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task<PowerDataDto> GetPowerDetailsAsync(string solarEdgeUri, PowerQuery powerQuery)
        {
            var apiKey = await _apiKey;

            var uri = new Uri(solarEdgeUri)
                .AppendPathSegments("site", $"{powerQuery.SiteId}", "powerDetails")
                .SetQueryParams(new
                {
                    startTime = powerQuery.StartDateTime,
                    endTime = powerQuery.EndDateTime,
                    meters = string.Join(',', EnumHelper.GetEnumValues<MeterType>()),
                    api_key = apiKey,
                    accept = "application/json"
                });

            try
            {
                _logger.LogDebug($"Requesting power details for site {powerQuery.SiteId} between {powerQuery.StartDateTime} and {powerQuery.EndDateTime}");

                var solarData = await uri.GetJsonAsync<PowerDataDto>();

                _logger.LogDebug("Success response received");
                
                return solarData;

            }
            catch (FlurlHttpException exception)
            {
                // 403 - forbidden
                // 429 - too many requests

                _logger.LogDebug($"Failed to get power data for site {powerQuery.SiteId}: {exception.Message}");

                throw new SolarEdgeResponseException(
                    (HttpStatusCode) exception.Call.Response.StatusCode,
                    powerQuery.SiteId,
                    powerQuery.StartDateTime,
                    powerQuery.EndDateTime);
            }
        }

        public async Task<EnergyDataDto> GetEnergyDetailsAsync(string solarEdgeUri, PowerQuery powerQuery)
        {
            var apiKey = await _apiKey;

            var uri = new Uri(solarEdgeUri)
                .AppendPathSegments("site", $"{powerQuery.SiteId}", "energyDetails")
                .SetQueryParams(new
                {
                    startTime = powerQuery.StartDateTime,
                    endTime = powerQuery.EndDateTime,
                    meters = string.Join(',', EnumHelper.GetEnumValues<MeterType>()),
                    timeUnit = "QUARTER_OF_AN_HOUR",
                    api_key = apiKey,
                    accept = "application/json"
                });

            try
            {
                _logger.LogDebug($"Requesting energy details for site {powerQuery.SiteId} between {powerQuery.StartDateTime} and {powerQuery.EndDateTime}");

                var solarData = await uri.GetJsonAsync<EnergyDataDto>();

                _logger.LogDebug("Success response received");

                return solarData;

            }
            catch (FlurlHttpException exception)
            {
                // 403 - forbidden
                // 429 - too many requests

                _logger.LogDebug($"Failed to get power data for site {powerQuery.SiteId}: {exception.Message}");

                throw new SolarEdgeResponseException(
                    (HttpStatusCode)exception.Call.Response.StatusCode,
                    powerQuery.SiteId,
                    powerQuery.StartDateTime,
                    powerQuery.EndDateTime);
            }
        }

        private static async Task<string> GetApiKeyAsync()            // todo: create a service that takes a paramName as input
        {
            using (var client = new AmazonSimpleSystemsManagementClient(Amazon.RegionEndpoint.APSoutheast2))
            {
                var request = new GetParameterRequest
                {
                    Name = Constants.Parameters.SolarEdgeApiKey
                };

                var response = await client.GetParameterAsync(request);

                return response.Parameter.Value;
            }
        }
    }
}