using AllOverIt.Helpers;
using Amazon.CDK;
using Amazon.CDK.AWS.IAM;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class Users : Construct
    {
        private const string DownloadUserName = "BucketDownloadUser";
        private const string UploadUserName = "BucketUploadUser";

        private readonly Iam _iam;

        public Users(Construct scope, Iam iam)
            : base(scope, "Users")
        {
            _iam = iam.WhenNotNull(nameof(iam));

            CreateUser(DownloadUserName, "DownloadS3", Constants.S3Buckets.UploadsBucketName);
            CreateUser(UploadUserName, "UploadS3", Constants.S3Buckets.UploadsBucketName);
        }

        private void CreateUser(string username, string policyId, string bucket)
        {
            var user = new User(this, username, new UserProps
            {
                UserName = username
            });

            var policy = new Policy(this, policyId, new PolicyProps
            {
                Statements = new[] { _iam.GetDownloadS3Policy(bucket) }
            });

            user.AttachInlinePolicy(policy);
        }
    }
}