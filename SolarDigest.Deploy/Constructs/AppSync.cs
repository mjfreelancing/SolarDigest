using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using SolarDigest.Deploy.Schema;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class AppSync : Construct
    {
        public AppSync(Construct scope, SolarDigestApiProps apiProps, AuthorizationMode authMode/*, IDataSourceRoleCache dataSourceRoleCache*/)
            : base(scope, "Api")
        {
            var graphQl = new SolarDigestGraphql(this, apiProps, authMode/*, dataSourceRoleCache*/);

            graphQl
                .AddSchemaQuery<ISolarDigestQueryDefinition>()
                .AddSchemaMutation<ISolarDigestMutationDefinition>();
            //.AddSchemaSubscription<ISolarDigestSubscriptionDefinition>();
        }
    }
}