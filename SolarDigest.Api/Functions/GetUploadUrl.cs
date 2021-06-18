using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Functions.Responses;
using SolarDigest.Api.Services;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class GetUploadUrl : FunctionBase<GetUploadUrlPayload, GetUploadUrlResponse>
    {
        protected override async Task<GetUploadUrlResponse> InvokeHandlerAsync(FunctionContext<GetUploadUrlPayload> context)
        {
            var urlCreator = context.ScopedServiceProvider.GetRequiredService<IPresignedUrlCreator>();

            var url = await urlCreator.CreateUploadUrlAsync(context.Payload.Filename);

            return new GetUploadUrlResponse { Url = url };
        }
    }
}