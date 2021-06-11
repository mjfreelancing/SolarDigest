using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SolarDigest.Api.Events;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class HydrateAllSitesPower : FunctionBase<HydrateAllSitesPowerPayload, bool>
    {
        protected override async Task<bool> InvokeHandlerAsync(FunctionContext<HydrateAllSitesPowerPayload> context)
        {
            var logger = context.Logger;
            logger.LogDebug("Hydrating power for all sites");

            AmazonEventBridgeClient client = null;

            var siteTable = context.ScopedServiceProvider.GetRequiredService<ISolarDigestSiteTable>();

            // only retrieve the properties we need
            var sites = siteTable!.GetAllSitesAsync(new[] {nameof(Site.Id)});

            await foreach (var site in sites)
            {
                logger.LogDebug($"Sending a hydration request for site {site.Id}");

                client ??= new AmazonEventBridgeClient();

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
                                SiteId = site.Id
                            })
                        }
                    }
                };

                await client.PutEventsAsync(putRequest).ConfigureAwait(false);
            }

            logger.LogDebug("All Sites have been iterated");

            return true;
        }
    }
}