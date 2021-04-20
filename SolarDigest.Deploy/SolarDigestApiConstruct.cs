using AllOverIt.Aws.Cdk.AppSync;
using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using SolarDigest.Deploy.Schema;

namespace SolarDigest.Deploy
{
    internal sealed class SolarDigestApiConstruct : ApiConstructBase
    {
        public SolarDigestApiConstruct(Construct scope, SolarDigestApiProps apiProps, AuthorizationMode authMode, IDataSourceRoleCache dataSourceRoleCache)
            : base(scope, apiProps, authMode, dataSourceRoleCache)
        {
            SolarDigestGraphqlApi
                .AddSchemaQuery<ISolarDigestQueryDefinition>()
                .AddSchemaMutation<ISolarDigestMutationDefinition>()
                .AddSchemaSubscription<ISolarDigestSubscriptionDefinition>();
        }
    }
}