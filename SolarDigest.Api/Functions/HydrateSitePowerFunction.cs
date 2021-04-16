using SolarDigest.Api.Payloads.EventBridge;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class HydrateSitePowerFunction : FunctionBase<HydrateSitePowerPayload, bool>
    {
        protected override Task<bool> InvokeHandlerAsync(FunctionContext<HydrateSitePowerPayload> context)
        {
            var siteId = context.Payload.Detail.Id;

            context.Logger.LogDebug($"Hydrating power for site Id '{siteId}'");

            return Task.FromResult(true);
        }
    }
}