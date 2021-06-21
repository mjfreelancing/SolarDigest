﻿using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.DependencyInjection;
using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Services;
using System.Net;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class GetUploadMultiPartAbort : FunctionBase<GetUploadMultiPartAbortPayload, bool>
    {
        protected override async Task<bool> InvokeHandlerAsync(FunctionContext<GetUploadMultiPartAbortPayload> context)
        {
            var logger = context.Logger;
            var payload = context.Payload;
            var filename = payload.Filename;

            var parameterStore = context.ScopedServiceProvider.GetRequiredService<IParameterStore>();

            var userSecretPath = $"{Constants.Parameters.SecretsRoot}/{Constants.Users.BucketUploadUser}";

            var response = await parameterStore.GetByPathAsync(userSecretPath).ConfigureAwait(false);

            response.TryGetValue($"{userSecretPath}/AccessKey", out var accessKey);
            response.TryGetValue($"{userSecretPath}/SecretKey", out var secretKey);

            var client = new AmazonS3Client(accessKey, secretKey);

            var uploadId = payload.UploadId;

            logger.LogDebug($"Aborting multi-part upload for file '{filename}', UploadId '{uploadId}'");

            var abortRequest = new AbortMultipartUploadRequest
            {
                UploadId = uploadId,
                BucketName = Constants.S3Buckets.UploadsBucketName,
                Key = filename
            };

            var abortResponse = await client.AbortMultipartUploadAsync(abortRequest).ConfigureAwait(false);

            logger.LogDebug($"Response = {abortResponse.HttpStatusCode}");

            return abortResponse.HttpStatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent;
        }
    }
}