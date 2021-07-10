using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Services;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    // if we ever need to return more than the URL, then encapsulate in a 'GetDownloadUrlResponse'
    public sealed class GetDownloadUrl : FunctionBase<GetDownloadUrlPayload, string>
    {
        protected override async Task<string> InvokeHandlerAsync(FunctionContext<GetDownloadUrlPayload> context)
        {
            var urlCreator = context.ScopedServiceProvider.GetRequiredService<IPresignedUrlCreator>();

            return await urlCreator.CreateDownloadUrlAsync(context.Payload.Filename);
        }
    }
}