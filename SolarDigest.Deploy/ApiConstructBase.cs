using AllOverIt.Aws.Cdk.AppSync;
using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;

namespace SolarDigest.Deploy
{
    internal abstract class ApiConstructBase : Construct
    {
        protected SolarDigestGraphqlApi SolarDigestGraphqlApi { get; }

        protected ApiConstructBase(Construct scope, SolarDigestApiProps apiProps, AuthorizationMode authMode, IDataSourceRoleCache dataSourceRoleCache)
            : base(scope, "Api")
        {
            SolarDigestGraphqlApi = new SolarDigestGraphqlApi(this, apiProps, authMode, dataSourceRoleCache);
        }
    }
}