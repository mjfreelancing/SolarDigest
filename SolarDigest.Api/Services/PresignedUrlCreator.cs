using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using SolarDigest.Api.Logging;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    internal sealed class PresignedUrlCreator : IPresignedUrlCreator
    {
        private readonly IParameterStore _parameterStore;
        private readonly ISolarDigestLogger _logger;

        public PresignedUrlCreator(IParameterStore parameterStore, ISolarDigestLogger logger)
        {
            _parameterStore = parameterStore.WhenNotNull(nameof(parameterStore));
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public Task<string> CreateDownloadUrlAsync(string name)
        {
            _logger.LogDebug($"Creating download pre-signed url for '{name}'");

            return CreateUrlAsync(Constants.S3Buckets.DownloadsBucketName, name, $"{Constants.Parameters.SecretsRoot}/{Constants.Users.BucketDownloadUser}",
                HttpVerb.GET);
        }

        public Task<string> CreateUploadUrlAsync(string name)
        {
            // partNumber is assumed to have a value if uploadId has a value (it is validated before getting here)
            _logger.LogDebug($"Creating upload pre-signed url for '{name}'");

            return CreateUrlAsync(Constants.S3Buckets.UploadsBucketName, name, $"{Constants.Parameters.SecretsRoot}/{Constants.Users.BucketUploadUser}",
                HttpVerb.PUT);
        }

        public Task<string> CreateUploadUrlAsync(string name, string uploadId, int partNumber)
        {
            // partNumber is assumed to have a value if uploadId has a value (it is validated before getting here)
            _logger.LogDebug(uploadId.IsNullOrEmpty()
                ? $"Creating upload pre-signed url for '{name}'"
                : $"Creating upload pre-signed url for '{name}' ({uploadId}/{partNumber})");

            return CreateUrlAsync(Constants.S3Buckets.UploadsBucketName, name, $"{Constants.Parameters.SecretsRoot}/{Constants.Users.BucketUploadUser}",
                HttpVerb.PUT, uploadId, partNumber);
        }

        private async Task<string> CreateUrlAsync(string bucket, string name, string userSecretPath, HttpVerb verb,
            string uploadId = default, int? partNumber = default)
        {
            var response = await _parameterStore.GetByPathAsync(userSecretPath);

            response.TryGetValue($"{userSecretPath}/AccessKey", out var accessKey);
            response.TryGetValue($"{userSecretPath}/SecretKey", out var secretKey);

            if (accessKey.IsNullOrEmpty() || secretKey.IsNullOrEmpty())
            {
                return string.Empty;
            }

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var s3Client = new AmazonS3Client(credentials, Constants.RegionEndpoint);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = name,
                Expires = DateTime.UtcNow.AddMinutes(5),
                Verb = verb
            };

            if (partNumber.HasValue)
            {
                request.UploadId = uploadId;
                request.PartNumber = partNumber.Value;
            }

            return s3Client.GetPreSignedURL(request);
        }
    }
}