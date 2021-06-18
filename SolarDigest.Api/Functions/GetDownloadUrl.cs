using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
            var parameterStore = context.ScopedServiceProvider.GetRequiredService<IParameterStore>();

            var response = await parameterStore.GetByPathAsync($"{Constants.Parameters.SecretsRoot}/BucketDownloadUser");

            return new GetDownloadUrlResponse { Url = JsonConvert.SerializeObject(response) };
        }
    }
}