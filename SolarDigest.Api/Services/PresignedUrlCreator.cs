using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Amazon.S3;
using SolarDigest.Api.Logging;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    internal sealed class PresignedUrlCreator : PresignedUrlCreatorBase
    {
        private readonly ISolarDigestLogger _logger;

        public PresignedUrlCreator(IParameterStore parameterStore, ISolarDigestLogger logger)
            : base(parameterStore)
        {
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public override Task<string> CreateDownloadUrlAsync(string name)
        {
            _logger.LogDebug($"Creating download pre-signed url for '{name}'");

            return CreateUrlAsync(Constants.S3Buckets.DownloadsBucketName, name, $"{Constants.Parameters.SecretsRoot}/{Constants.Users.BucketDownloadUser}",
                HttpVerb.GET);
        }

        public override Task<string> CreateUploadUrlAsync(string name, string uploadId, int? partNumber)
        {
            // partNumber is assumed to have a value if uploadId has a value (it is validated before getting here)
            _logger.LogDebug(uploadId.IsNullOrEmpty()
                ? $"Creating upload pre-signed url for '{name}'"
                : $"Creating upload pre-signed url for '{name}' ({uploadId}/{partNumber})");

            return CreateUrlAsync(Constants.S3Buckets.UploadsBucketName, name, $"{Constants.Parameters.SecretsRoot}/{Constants.Users.BucketUploadUser}",
                HttpVerb.PUT, uploadId, partNumber);
        }
    }
}