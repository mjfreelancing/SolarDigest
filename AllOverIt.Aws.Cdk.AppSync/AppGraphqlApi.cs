using AllOverIt.Aws.Cdk.AppSync.Factories;
using AllOverIt.Aws.Cdk.AppSync.MappingTemplates;
using AllOverIt.Aws.Cdk.AppSync.Schema;
using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;

namespace AllOverIt.Aws.Cdk.AppSync
{
    public abstract class AppGraphqlApi : GraphqlApi
    {
        private readonly ISchemaBuilder _schemaBuilder;

        public AppGraphqlApi(Construct scope, string id, IGraphqlApiProps apiProps, IMappingTemplates mappingTemplates)
            : base(scope, id, apiProps)
        {
            var dataSourceFactory = new DataSourceFactory(this);
            var resolverFactory = new ResolverFactory(this, mappingTemplates, dataSourceFactory);
            var gqlTypeCache = new GraphqlTypeStore(this, mappingTemplates, dataSourceFactory, resolverFactory);
            _schemaBuilder = new SchemaBuilder(this, mappingTemplates, gqlTypeCache, dataSourceFactory);
        }

        public AppGraphqlApi AddSchemaQuery<TType>() where TType : IQueryDefinition
        {
            _schemaBuilder.AddQuery<TType>();
            return this;
        }

        public AppGraphqlApi AddSchemaMutation<TType>() where TType : IMutationDefinition
        {
            _schemaBuilder.AddMutation<TType>();
            return this;
        }

        public AppGraphqlApi AddSchemaSubscription<TType>() where TType : ISubscriptionDefinition
        {
            _schemaBuilder.AddSubscription<TType>();
            return this;
        }
    }
}