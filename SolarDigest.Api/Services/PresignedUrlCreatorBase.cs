using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    internal abstract class PresignedUrlCreatorBase : IPresignedUrlCreator
    {
        private readonly IParameterStore _parameterStore;

        protected PresignedUrlCreatorBase(IParameterStore parameterStore)
        {
            _parameterStore = parameterStore.WhenNotNull(nameof(parameterStore));
        }

        public abstract Task<string> CreateDownloadUrlAsync(string name);
        public abstract Task<string> CreateUploadUrlAsync(string name, string uploadId, int? partNumber);

        protected async Task<string> CreateUrlAsync(string bucket, string name, string userSecretPath, HttpVerb verb,
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