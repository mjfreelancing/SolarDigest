using AllOverIt.Aws.Cdk.AppSync;
using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using SolarDigest.Deploy.Schema;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class AppSync : Construct
    {
        public AppSync(Construct scope, SolarDigestAppProps appProps, AuthorizationMode authMode, IMappingTemplates mappingTemplates)
            : base(scope, "AppSync")
        {
            var graphql = new SolarDigestGraphql(this, appProps, authMode, mappingTemplates);

            graphql
                .AddSchemaQuery<ISolarDigestQueryDefinition>()
                .AddSchemaMutation<ISolarDigestMutationDefinition>();
                //.AddSchemaSubscription<ISolarDigestSubscriptionDefinition>();
        }
    }
}