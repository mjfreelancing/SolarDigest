using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using SolarDigest.Deploy.Schema;

namespace SolarDigest.Deploy.Constructs
{
    //internal sealed class SolarDigestApiConstruct : ApiConstructBase
    //{
    //    public SolarDigestApiConstruct(Construct scope, SolarDigestApiProps apiProps, AuthorizationMode authMode, IDataSourceRoleCache dataSourceRoleCache)
    //        : base(scope, apiProps, authMode, dataSourceRoleCache)
    //    {
    //        SolarDigestGraphqlApi
    //            .AddSchemaQuery<ISolarDigestQueryDefinition>();
    //            //.AddSchemaMutation<ISolarDigestMutationDefinition>()
    //            //.AddSchemaSubscription<ISolarDigestSubscriptionDefinition>();
    //    }
    //}



    internal sealed class AppSync : Construct
    {
        //protected SolarDigestGraphql SolarDigestGraphql { get; }

        public AppSync(Construct scope, SolarDigestApiProps apiProps, AuthorizationMode authMode/*, IDataSourceRoleCache dataSourceRoleCache*/)
            : base(scope, "Api")
        {
            var graphQl = new SolarDigestGraphql(this, apiProps, authMode/*, dataSourceRoleCache*/);

            graphQl
                .AddSchemaQuery<ISolarDigestQueryDefinition>();
            //.AddSchemaMutation<ISolarDigestMutationDefinition>()
            //.AddSchemaSubscription<ISolarDigestSubscriptionDefinition>();
        }
    }
}