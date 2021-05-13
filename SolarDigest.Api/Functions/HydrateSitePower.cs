using AllOverIt.Extensions;
using AllOverIt.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Payloads.EventBridge;
using SolarDigest.Api.Repository;
using SolarDigest.Api.Services.SolarEdge;
using SolarDigest.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class HydrateSitePower : FunctionBase<HydrateSitePowerPayload, bool>
    {
        protected override async Task<bool> InvokeHandlerAsync(FunctionContext<HydrateSitePowerPayload> context)
        {
            var request = context.Payload.Detail;

            // todo: add request validation

            var siteId = request.Id;

            context.Logger.LogDebug($"Hydrating power for site Id '{siteId}'");

            // get site info
            var siteTable = context.ScopedServiceProvider.GetService<ISolarDigestSiteTable>();
            var site = await siteTable!.GetItemAsync<Site>(siteId);


            // determine the refresh period to hydrate (local time)
            var lastRefreshDateTime = site.LastRefreshDateTime.IsNullOrEmpty()
                ? site.StartDate.ParseSolarDate()
                : site.LastRefreshDateTime.ParseSolarDateTime();

            var hydrateStartDateTime = lastRefreshDateTime;
            var hydrateEndDateTime = site.UtcToLocalTime(DateTime.UtcNow).AddHours(-1).TrimToHour();

            context.Logger.LogDebug($"Site '{siteId}' will be hydrating for the period {hydrateStartDateTime.GetSolarDateTimeString()} to " +
                                    $"{hydrateEndDateTime.GetSolarDateTimeString()} (local time)");



            // todo: for testing only
            hydrateEndDateTime = hydrateStartDateTime.AddHours(1);


            // get the power / energy data
            var solarEdgeApi = context.ScopedServiceProvider.GetService<ISolarEdgeApi>();

            var powerQuery = new PowerQuery
            {
                SiteId = siteId,
                StartDateTime = hydrateStartDateTime.GetSolarDateTimeString(),
                EndDateTime = hydrateEndDateTime.GetSolarDateTimeString()
            };

            var (powerResults, energyResults) = await TaskHelper.WhenAll(
                solarEdgeApi!.GetPowerDetailsAsync(Constants.SolarEdge.MonitoringUri, powerQuery),
                solarEdgeApi!.GetEnergyDetailsAsync(Constants.SolarEdge.MonitoringUri, powerQuery)
            );





            var meterCount = powerResults.PowerDetails.Meters.Count();
            var energyCount = energyResults.EnergyDetails.Meters.Count();
            
            context.Logger.LogDebug($"Received {meterCount} power meter and {energyCount} energy meter results");



            return true;
        }
    }
}