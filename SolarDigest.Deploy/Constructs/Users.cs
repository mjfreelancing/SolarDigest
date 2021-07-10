using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using System.Collections.Generic;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class Users : Construct
    {
        // Caching users that need their access/secret keys to be added to the ParameterStore
        internal readonly IDictionary<string, CfnAccessKey> UserAccessKeys = new Dictionary<string, CfnAccessKey>();

        internal const string UploadUserName = "BucketUploadUser";
        internal const string DownloadUserName = "BucketDownloadUser";

        public Users(Construct scope, Iam iam)
            : base(scope, "Users")
        {
            CreateUser(UploadUserName, iam.GetUploadS3PolicyStatement(Constants.S3Buckets.UploadsBucketName), true);
            CreateUser(DownloadUserName, iam.GetDownloadS3PolicyStatement(Constants.S3Buckets.DownloadsBucketName), true);
        }

        private void CreateUser(string username, PolicyStatement policyStatement, bool addToCache)
        {
            var user = new User(this, username, new UserProps
            {
                UserName = username
            });

            var policy = new Policy(this, $"{username}Policy", new PolicyProps
            {
                Statements = new[] { policyStatement }
            });

            user.AttachInlinePolicy(policy);

            var accessKey = new CfnAccessKey(this, $"{username}AccessKey", new CfnAccessKeyProps
            {
                UserName = user.UserName
            });

            if (addToCache)
            {
                UserAccessKeys.Add(username, accessKey);
            }
        }
    }
}