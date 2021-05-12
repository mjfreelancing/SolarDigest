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
                            Id = "1514817",
                            //StartDate = $"{DateTime.Now.AddDays(-1):yyyy-MM-dd}",
                            //EndDate = $"{DateTime.Now:yyyy-MM-dd}"
                        })
                    }
                }
            };

            var client = new AmazonEventBridgeClient();

            await client.PutEventsAsync(putRequest);

            return true;
        }
    }
}