using System.Threading.Tasks;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Functions.Responses;

namespace SolarDigest.Api.Functions
{
    public sealed class GetUploadUrl : FunctionBase<GetUploadUrlPayload, GetUploadUrlResponse>
    {
        protected override Task<GetUploadUrlResponse> InvokeHandlerAsync(FunctionContext<GetUploadUrlPayload> context)
        {
            return Task.FromResult(new GetUploadUrlResponse { Url = "UploadUrlHere" });
        }
    }
}