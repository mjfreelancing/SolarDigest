using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Functions.Responses;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class GetDownloadUrl : FunctionBase<GetDownloadUrlPayload, GetDownloadUrlResponse>
    {
        protected override Task<GetDownloadUrlResponse> InvokeHandlerAsync(FunctionContext<GetDownloadUrlPayload> context)
        {
            return Task.FromResult(new GetDownloadUrlResponse{ Url = "DownloadUrlHere" });
        }
    }
}