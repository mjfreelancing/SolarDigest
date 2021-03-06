using AllOverIt.Helpers;
using Flurl;
using Flurl.Http;
using Polly;
using Polly.Retry;
using SolarDigest.Api.Exceptions;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Services.SolarEdge.Response;
using SolarDigest.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services.SolarEdge
{
    internal sealed class SolarEdgeApi : ISolarEdgeApi
    {
        private readonly AsyncRetryPolicy _httpRetryPolicy = Policy
            .Handle<FlurlHttpTimeoutException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(500 * retryAttempt));

        private readonly ISolarDigestLogger _logger;

        public SolarEdgeApi(ISolarDigestLogger logger)
        {
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task<PowerDataDto> GetPowerDetailsAsync(string apiKey, PowerQuery powerQuery)
        {
            var uri = new Uri(Constants.SolarEdge.MonitoringUri)
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
                _logger.LogDebug(
                    $"Requesting power details for site {powerQuery.SiteId} between {powerQuery.StartDateTime} and {powerQuery.EndDateTime}");

                var solarData = await _httpRetryPolicy.ExecuteAsync(() => uri.GetJsonAsync<PowerDataDto>());

                _logger.LogDebug("Power details received successfully");

                return solarData;
            }
            catch (FlurlHttpTimeoutException exception)
            {
                _logger.LogDebug($"Timed out while getting power data for site {powerQuery.SiteId}: {exception.Message}");

                throw new SolarEdgeTimeoutException(powerQuery.SiteId, powerQuery.StartDateTime, powerQuery.EndDateTime);
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

        public async Task<EnergyDataDto> GetEnergyDetailsAsync(string apiKey, PowerQuery powerQuery)
        {
            var uri = new Uri(Constants.SolarEdge.MonitoringUri)
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

                var solarData = await _httpRetryPolicy.ExecuteAsync(() => uri.GetJsonAsync<EnergyDataDto>());

                _logger.LogDebug("Energy details received successfully");

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
    }
}