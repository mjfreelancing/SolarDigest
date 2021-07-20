using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using SolarDigest.Shared.Utils;
using System.Collections.Generic;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class Users : Construct
    {
        // Caching users that need their access/secret keys to be added to the ParameterStore
        internal readonly IDictionary<string, CfnAccessKey> UserAccessKeys = new Dictionary<string, CfnAccessKey>();

        public Users(Construct scope, Iam iam)
            : base(scope, "Users")
        {
            CreateUser(Shared.Constants.Users.BucketUploadUser, iam.GetUploadS3PolicyStatement(Shared.Constants.S3Buckets.UploadsBucketName), true);
            CreateUser(Shared.Constants.Users.BucketDownloadUser, iam.GetDownloadS3PolicyStatement(Shared.Constants.S3Buckets.DownloadsBucketName), true);
        }

        private void CreateUser(string username, PolicyStatement policyStatement, bool addToCache)
        {
            var appUsername = $"{Helpers.GetAppVersionName()}_{username}";

            var user = new User(this, username, new UserProps
            {
                UserName = appUsername
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
                UserAccessKeys.Add(appUsername, accessKey);
            }
        }
    }
}