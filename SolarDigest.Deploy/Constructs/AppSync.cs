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
            var graphQl = new SolarDigestGraphql(this, appProps, authMode, mappingTemplates);

            graphQl
                // consider naming convention overrides:
                //  - such as dropping I from interface names for types
                //  - such as auto adding 'Input' suffix to input types
                //  - such as strip I's of interfaces yet leave abstract classes as-is
                .AddSchemaQuery<ISolarDigestQueryDefinition>()
                .AddSchemaMutation<ISolarDigestMutationDefinition>();
            //.AddSchemaSubscription<ISolarDigestSubscriptionDefinition>();
        }
    }
}