﻿using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Repository;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions.GetSite
{
    /*

    query MyQuery {
      site(id: "1514817") {
        power(filter: {meterType: PRODUCTION, summaryType: AVERAGE, startDate: "2021-01-01", endDate: "2021-01-01"}) {
          nextToken
          watts {
            watts
            wattHour
            time
          }
        }
        lastRefreshDateTime
      }
    }

    mutation MyMutation {
      addSite(id: "1514817", site: {apiKey: "api-key-here", contactEmail: "malcolm@mjfreelancing.com", contactName: "Malcolm Smith", lastAggregationDate: "2020-05-09", lastRefreshDateTime: "2020-05-09", lastSummaryDate: "2020-05-09", startDate: "2020-05-09", timeZoneId: "AUS Eastern Standard Time"}) {
        id
        contactEmail
        contactName
      }
    }

    */

    public sealed class GetSite : FunctionBase<GetSitePayload, GetSiteResponse>
    {
        protected override async Task<GetSiteResponse> InvokeHandlerAsync(FunctionContext<GetSitePayload> context)
        {
            var logger = context.Logger;
            var payload = context.Payload;

            logger.LogDebug($"Reading site info for Id '{payload.Id}'");

            var siteTable = context.ScopedServiceProvider.GetRequiredService<ISolarDigestSiteTable>();

            var site = await siteTable!.GetSiteAsync(payload.Id).ConfigureAwait(false);

            var mapper = context.ScopedServiceProvider.GetRequiredService<IMapper>();
            return mapper.Map<GetSiteResponse>(site);
        }
    }
}