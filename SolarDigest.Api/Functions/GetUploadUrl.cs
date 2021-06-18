using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
            var parameterStore = context.ScopedServiceProvider.GetRequiredService<IParameterStore>();

            var response = await parameterStore.GetByPathAsync($"{Constants.Parameters.SecretsRoot}/BucketUploadUser");

            return new GetUploadUrlResponse { Url = JsonConvert.SerializeObject(response) };
        }
    }
}