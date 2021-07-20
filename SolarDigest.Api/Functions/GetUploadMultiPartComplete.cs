using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Services;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class GetUploadMultiPartComplete : FunctionBase<GetUploadMultiPartCompletePayload, bool>
    {
        protected override async Task<bool> InvokeHandlerAsync(FunctionContext<GetUploadMultiPartCompletePayload> context)
        {
            var logger = context.Logger;
            var payload = context.Payload;
            var uploadId = payload.UploadId;
            var filename = payload.Filename;

            logger.LogDebug($"Completing multi-part upload for file '{filename}', UploadId '{uploadId}'");

            var compRequest = new CompleteMultipartUploadRequest
            {
                BucketName = Shared.Constants.S3Buckets.UploadsBucketName,
                Key = filename,
                UploadId = uploadId,
                PartETags = payload.ETags
                    .Select(item =>
                        new PartETag
                        {
                            ETag = item.ETag,
                            PartNumber = item.PartNumber
                        })
                    .ToList()
            };

            var parameterStore = context.ScopedServiceProvider.GetRequiredService<IParameterStore>();

            var userSecretPath = $"{Constants.Parameters.SecretsRoot}/{Shared.Constants.Users.BucketUploadUser}";

            var response = await parameterStore.GetByPathAsync(userSecretPath).ConfigureAwait(false);

            response.TryGetValue($"{userSecretPath}/AccessKey", out var accessKey);
            response.TryGetValue($"{userSecretPath}/SecretKey", out var secretKey);

            var client = new AmazonS3Client(accessKey, secretKey);

            CompleteMultipartUploadResponse completeResponse;

            try
            {
                completeResponse = await client.CompleteMultipartUploadAsync(compRequest).ConfigureAwait(false);

                logger.LogDebug($"Response = {completeResponse.HttpStatusCode}");
            }
            catch (AmazonS3Exception exception)
            {
                // Example error:
                // One or more of the specified parts could not be found. The part may not have been uploaded, or the specified entity tag may not match the part's entity tag.
                logger.LogException(exception);
                
                // todo: turn this into an error response
                return false;
            }

            return completeResponse.HttpStatusCode is HttpStatusCode.OK;
        }
    }
}