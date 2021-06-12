using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Models;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{

    /*

    {
      "version" : "2017-02-28",
      "operation": "Invoke",
      "payload": { 
        "id": $context.source.id,
        "meterType": $context.arguments.filter.meterType,
        "summaryType": $context.arguments.filter.summaryType
      }
    }

    */

    // For the purpose of CDK deployment, all functions need to reside in the same (SolarDigest.Api.Functions) namespace.

    public sealed class GetSitePowerSummary : FunctionBase<GetSitePowerSummaryPayload, SitePower>
    {
        protected override Task<SitePower> InvokeHandlerAsync(FunctionContext<GetSitePowerSummaryPayload> context)
        {
            var serviceProvider = context.ScopedServiceProvider;
            var payload = context.Payload;

            //serviceProvider.InvokeValidator<GetSitePowerSummaryPayloadValidator, GetSitePowerSummaryPayload>(payload);

            var logger = context.Logger;

            logger.LogDebug($"Getting power summary ({payload.MeterType}, {payload.SummaryType}) for site Id " +
                            $"{payload.Id} between {payload.StartDate} and {payload.EndDate}");




            return Task.FromResult(new SitePower());
        }
    }
}