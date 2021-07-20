using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.MappingTemplates;
using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;

namespace SolarDigest.Deploy
{
    internal sealed class SolarDigestGraphql : AppGraphqlBase
    {
        public SolarDigestGraphql(Construct scope, IAuthorizationMode authMode, IMappingTemplates mappingTemplates)
            : base(scope, "GraphQl", new GraphqlApiProps
            {
                Name = $"{Shared.Constants.AppName} V{Shared.Constants.ServiceVersion}",
                AuthorizationConfig = new AuthorizationConfig { DefaultAuthorization = authMode }
            }, mappingTemplates)
        {
        }
    }
}