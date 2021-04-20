using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Factories;
using AllOverIt.Aws.Cdk.AppSync.Schema;
using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;

namespace SolarDigest.Deploy
{
    internal sealed class SolarDigestGraphqlApi : GraphqlApi
    {
        private readonly ISchemaBuilder _schemaBuilder;

        public SolarDigestGraphqlApi(Construct scope, SolarDigestApiProps apiProps, IAuthorizationMode authMode, IDataSourceRoleCache serviceRoles)
            : base(scope, "Graphql", new GraphqlApiProps
            {
                Name = $"{apiProps.AppName} V{apiProps.Version}",
                AuthorizationConfig = new AuthorizationConfig { DefaultAuthorization = authMode }
            })
        {
            // these require the GraphqlApi reference
            var dataSourceFactory = new DataSourceFactory(this, serviceRoles);
            var resolverFactory = new ResolverFactory(this, apiProps.MappingTemplates, dataSourceFactory);
            var gqlTypeCache = new GraphqlTypeStore(resolverFactory);
            _schemaBuilder = new SchemaBuilder(this, apiProps.MappingTemplates, gqlTypeCache, dataSourceFactory);
        }

        public SolarDigestGraphqlApi AddSchemaQuery<TType>() where TType : IQueryDefinition
        {
            _schemaBuilder.AddQuery<TType>();
            return this;
        }

        public SolarDigestGraphqlApi AddSchemaMutation<TType>() where TType : IMutationDefinition
        {
            _schemaBuilder.AddMutation<TType>();
            return this;
        }

        public SolarDigestGraphqlApi AddSchemaSubscription<TType>() where TType : ISubscriptionDefinition
        {
            _schemaBuilder.AddSubscription<TType>();
            return this;
        }
    }
}