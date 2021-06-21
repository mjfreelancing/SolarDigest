using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Services;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    // if we ever need to return more than the URL, then encapsulate in a 'GetUploadUrlResponse'
    public sealed class GetUploadUrl : FunctionBase<GetUploadUrlPayload, string>
    {
        protected override async Task<string> InvokeHandlerAsync(FunctionContext<GetUploadUrlPayload> context)
        {
            var urlCreator = context.ScopedServiceProvider.GetRequiredService<IPresignedUrlCreator>();

            var payload = context.Payload;

            return await urlCreator.CreateUploadUrlAsync(payload.Filename);
        }
    }
}