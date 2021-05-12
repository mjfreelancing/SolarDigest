using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Payloads.EventBridge;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
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

            var siteTable = context.ScopedServiceProvider.GetService<ISolarDigestSiteTable>();
            var site = await siteTable!.GetItemAsync<Site>(siteId);




            // local time
            //var lastRefreshDateTime = site.LastRefreshDateTime.IsNullOrEmpty()
            //    ? site.StartDate.ParseSolarDate()
            //    : site.LastRefreshDateTime.ParseSolarDateTime();

            //var hydrateStartDateTime = lastRefreshDateTime;
            //var hydrateEndDateTime = site.UtcToLocalTime(DateTime.UtcNow).AddHours(-1);

            //context.Logger.LogDebug($"Site '{siteId}' will be hydrating for the period {hydrateStartDateTime.GetSolarDateTimeString()} to " +
            //                        $"{hydrateEndDateTime.GetSolarDateTimeString()} (local time)");







            //var solarEdgeApi = context.ScopedServiceProvider.GetService<ISolarEdgeApi>();


            //var powerResults = await solarEdgeApi!.GetPowerDetailsAsync(Constants.SolarEdge.MonitoringUri, new PowerQuery
            //{
            //    SiteId = siteId,
            //    StartDateTime = "2020-07-09 20:00:00",
            //    EndDateTime = "2020-07-09 21:00:00"
            //});

            //var meterCount1 = powerResults.PowerDetails.Meters.Count();
            //context.Logger.LogDebug($"Received {meterCount1} power results");



            return true;
        }
    }
}