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

        internal CfnAccessKey UploadUserAccessKey { get; }
        internal CfnAccessKey DownloadUserAccessKey { get; }

        public Users(Construct scope, Iam iam)
            : base(scope, "Users")
        {
            _iam = iam.WhenNotNull(nameof(iam));

            UploadUserAccessKey = CreateUser(UploadUserName, "UploadS3", _iam.GetUploadS3PolicyStatement(Constants.S3Buckets.UploadsBucketName));
            DownloadUserAccessKey = CreateUser(DownloadUserName, "DownloadS3", _iam.GetDownloadS3PolicyStatement(Constants.S3Buckets.DownloadsBucketName));
        }

        private CfnAccessKey CreateUser(string username, string policyId, PolicyStatement policyStatement)
        {
            var user = new User(this, username, new UserProps
            {
                UserName = username
            });

            var policy = new Policy(this, policyId, new PolicyProps
            {
                Statements = new[] { policyStatement }
            });

            user.AttachInlinePolicy(policy);

            return new CfnAccessKey(this, $"{username}AccessKey", new CfnAccessKeyProps
            {
                UserName = username
            });
        }
    }
}