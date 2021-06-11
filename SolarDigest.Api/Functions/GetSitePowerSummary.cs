using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Models;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{

    /*

    {
      "version" : "2017-02-28",
      "operation": "Invoke",
      "payload": $util.toJson({
          "id": $ctx.source.id,
          "meterType": $context.arguments.meterType,
          "summaryType": $context.arguments.summaryType
      })
    }

    */

    public sealed class GetSitePowerSummary : FunctionBase<GetSitePowerSummaryPayload, SitePower>
    {
        protected override Task<SitePower> InvokeHandlerAsync(FunctionContext<GetSitePowerSummaryPayload> context)
        {
            return Task.FromResult(new SitePower());
        }
    }
}