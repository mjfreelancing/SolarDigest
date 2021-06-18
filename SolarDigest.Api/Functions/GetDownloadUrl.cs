using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Functions.Responses;
using SolarDigest.Api.Services;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class GetDownloadUrl : FunctionBase<GetDownloadUrlPayload, GetDownloadUrlResponse>
    {
        protected override async Task<GetDownloadUrlResponse> InvokeHandlerAsync(FunctionContext<GetDownloadUrlPayload> context)
        {
            var urlCreator = context.ScopedServiceProvider.GetRequiredService<IPresignedUrlCreator>();

            var url = await urlCreator.CreateDownloadUrlAsync(context.Payload.Filename);

            return new GetDownloadUrlResponse { Url = url };
        }
    }
}