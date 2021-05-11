using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Api.Payloads.EventBridge;
using SolarDigest.Api.Services.SolarEdge;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class HydrateSitePower : FunctionBase<HydrateSitePowerPayload, bool>
    {
        protected override async Task<bool> InvokeHandlerAsync(FunctionContext<HydrateSitePowerPayload> context)
        {
            var siteId = context.Payload.Detail.Id;

            context.Logger.LogDebug($"Hydrating power for site Id '{siteId}'");

            var solarEdgeApi = context.ScopedServiceProvider.GetService<ISolarEdgeApi>();



            if (siteId == "1")
            {
                var powerResults = await solarEdgeApi!.GetPowerDetailsAsync(Constants.SolarEdge.MonitoringUri, new PowerQuery
                {
                    SiteId = "1514817",
                    StartDateTime = "2020-07-09 20:00:00",
                    EndDateTime = "2020-07-09 21:00:00"
                });

                if (powerResults.StatusCode == HttpStatusCode.OK)
                {
                    var meterCount = powerResults.PowerData.PowerDetails.Meters.Count();
                    context.Logger.LogDebug($"Received {meterCount} power results");
                }
                else
                {
                    context.Logger.LogDebug($"Failed to call SolarEdge, status code {powerResults.StatusCode}");
                }





                var energyResults = await solarEdgeApi!.GetEnergyDetailsAsync(Constants.SolarEdge.MonitoringUri, new PowerQuery
                {
                    SiteId = "1514817",
                    StartDateTime = "2020-07-09 20:00:00",
                    EndDateTime = "2020-07-09 21:00:00"
                });

                if (energyResults.StatusCode == HttpStatusCode.OK)
                {
                    var meterCount = energyResults.EnergyData.EnergyDetails.Meters.Count();
                    context.Logger.LogDebug($"Received {meterCount} energy results");
                }
                else
                {
                    context.Logger.LogDebug($"Failed to call SolarEdge, status code {energyResults.StatusCode}");
                }
            }







            return true;
        }
    }
}