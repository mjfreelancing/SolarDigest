using System.Threading.Tasks;
using Amazon.S3;
using SolarDigest.Api.Services;

namespace SolarDigest.Api.Functions
{
    internal sealed class PresignedUrlCreator : PresignedUrlCreatorBase
    {
        public PresignedUrlCreator(IParameterStore parameterStore)
            : base(parameterStore)
        {
        }

        public override Task<string> CreateDownloadUrlAsync(string name)
        {
            return CreateUrlAsync(Constants.S3Buckets.DownloadsBucketName, name, $"{Constants.Parameters.SecretsRoot}/{Constants.Users.BucketDownloadUser}", HttpVerb.GET);
        }

        public override Task<string> CreateUploadUrlAsync(string name)
        {
            return CreateUrlAsync(Constants.S3Buckets.UploadsBucketName, name, $"{Constants.Parameters.SecretsRoot}/{Constants.Users.BucketUploadUser}", HttpVerb.PUT);
        }
    }
}