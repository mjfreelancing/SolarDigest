using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SSM;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class ParameterStore : Construct
    {
        public ParameterStore(Construct scope, Users users)
            : base(scope, "Parameters")
        {
            CreateAccessKeyParameters(users.UploadUserAccessKey);
            CreateAccessKeyParameters(users.DownloadUserAccessKey);
        }

        private void CreateAccessKeyParameters(CfnAccessKey accessKey)
        {
            // NOTE: Secure strings are not supported by Cloud Formation. Something to revisit.

            _ = new StringParameter(this, $"{accessKey.UserName}AccessKey", new StringParameterProps
            {
                Tier = ParameterTier.STANDARD,
                Type = ParameterType.STRING,
                ParameterName = $"/Secrets/{accessKey.UserName}/AccessKey",
                StringValue = accessKey.Ref
            });

            _ = new StringParameter(this, $"{accessKey.UserName}SecretKey", new StringParameterProps
            {
                Tier = ParameterTier.STANDARD,
                Type = ParameterType.STRING,
                ParameterName = $"/Secrets/{accessKey.UserName}/SecretKey",
                StringValue = accessKey.AttrSecretAccessKey
            });
        }
    }
}