using Amazon.CDK;
using Amazon.CDK.AWS.SSM;
using System;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class ParameterStore : Construct
    {
        public ParameterStore(Construct scope, Users users)
            : base(scope, "Parameters")
        {
            CreateAccessKeyParameters(Shared.Constants.Users.BucketUploadUser, users);
            CreateAccessKeyParameters(Shared.Constants.Users.BucketDownloadUser, users);
        }

        private void CreateAccessKeyParameters(string username, Users users)
        {
            // NOTE: Secure strings are not supported by Cloud Formation. Something to revisit.
            var appUsername = $"{Shared.Helpers.GetAppVersionName()}_{username}";

            if (!users.UserAccessKeys.TryGetValue(appUsername, out var accessKey))
            {
                throw new InvalidOperationException($"The user '{username}' does not have a cached access key");
            }

            // Note: cannot use accessKey.UserName in place of username

            _ = new StringParameter(this, $"{username}AccessKey", new StringParameterProps
            {
                Tier = ParameterTier.STANDARD,
                Type = ParameterType.STRING,
                ParameterName = $"/Secrets/{appUsername}/AccessKey",
                StringValue = accessKey.Ref
            });

            _ = new StringParameter(this, $"{username}SecretKey", new StringParameterProps
            {
                Tier = ParameterTier.STANDARD,
                Type = ParameterType.STRING,
                ParameterName = $"/Secrets/{appUsername}/SecretKey",
                StringValue = accessKey.AttrSecretAccessKey
            });
        }
    }
}