using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Newtonsoft.Json;
using SolarDigest.Api.Events;
using SolarDigest.Api.Payloads.EventBridge;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class HydrateAllSitesPower : FunctionBase<HydrateAllSitesPowerPayload, bool>
    {
        protected override async Task<bool> InvokeHandlerAsync(FunctionContext<HydrateAllSitesPowerPayload> context)
        {
            context.Logger.LogDebug("Hydrating power for all sites");

            // todo: query all available sites and create a suitable request (max 10 per request)

            var putRequest = new PutEventsRequest
            {
                Entries =
                {
                    new PutEventsRequestEntry
                    {
                        Source = Constants.Events.Source,
                        EventBusName = "default",
                        DetailType = nameof(HydrateSitePowerEvent),
                        Time = DateTime.Now,
                        Detail = JsonConvert.SerializeObject(new HydrateSitePowerEvent
                        {
                            SiteId = "1514817"

                            // only for testing - these are only used when forcing a refresh (and the timestamps need to be in local time)
                            // StartDateTime = $"{DateTime.Now.AddDays(-1).AddHours(-1):yyyy-MM-dd HH:mm:ss}",
                            // EndDateTime = $"{DateTime.Now.AddDays(-1):yyyy-MM-dd HH:mm:ss}"
                        })
                    }
                }
            };

            var client = new AmazonEventBridgeClient();

            await client.PutEventsAsync(putRequest).ConfigureAwait(false);

            return true;
        }
    }
}