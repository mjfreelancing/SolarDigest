using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Functions.Responses;
using SolarDigest.Api.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class GetUploadMultiPartUrls : FunctionBase<GetUploadMultiPartUrlsPayload, GetUploadMultiPartsResponse>
    {
        protected override async Task<GetUploadMultiPartsResponse> InvokeHandlerAsync(FunctionContext<GetUploadMultiPartUrlsPayload> context)
        {
            var payload = context.Payload;
            var filename = payload.Filename;

            var parameterStore = context.ScopedServiceProvider.GetRequiredService<IParameterStore>();

            var userSecretPath = $"{Constants.Parameters.SecretsRoot}/{Shared.Constants.Users.BucketUploadUser}";

            var parameters = await parameterStore.GetByPathAsync(userSecretPath).ConfigureAwait(false);

            parameters.TryGetValue($"{userSecretPath}/AccessKey", out var accessKey);
            parameters.TryGetValue($"{userSecretPath}/SecretKey", out var secretKey);

            var client = new AmazonS3Client(accessKey, secretKey);

            var initRequest = new InitiateMultipartUploadRequest
            {
                BucketName = Shared.Constants.S3Buckets.UploadsBucketName,
                Key = filename
            };

            var initResponse = await client.InitiateMultipartUploadAsync(initRequest).ConfigureAwait(false);
            var uploadId = initResponse.UploadId;

            var urlCreator = context.ScopedServiceProvider.GetRequiredService<IPresignedUrlCreator>();

            var parts = new List<GetUploadMultiPartResponse>();

            for (var partNumber = 1; partNumber <= payload.PartCount; partNumber++)
            {
                var part = new GetUploadMultiPartResponse
                {
                    PartNumber = partNumber,
                    Url = await urlCreator.CreateUploadUrlAsync(payload.Filename, uploadId, partNumber)
                };

                parts.Add(part);
            }

            return new GetUploadMultiPartsResponse
            {
                UploadId = uploadId,
                Parts = parts
            };
        }
    }
}