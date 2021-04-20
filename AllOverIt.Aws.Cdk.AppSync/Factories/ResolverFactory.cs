using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Aws.Cdk.AppSync.Extensions;
using AllOverIt.Helpers;
using Amazon.CDK.AWS.AppSync;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace AllOverIt.Aws.Cdk.AppSync.Factories
{
    public sealed class ResolverFactory : IResolverFactory
    {
        private readonly GraphqlApi _graphqlApi;
        private readonly IMappingTemplates _mappingTemplates;
        private readonly IDataSourceFactory _dataSourceFactory;

        public ResolverFactory(GraphqlApi graphqlApi, IMappingTemplates mappingTemplates, IDataSourceFactory dataSourceFactory)
        {
            _graphqlApi = graphqlApi.WhenNotNull(nameof(graphqlApi));
            _mappingTemplates = mappingTemplates.WhenNotNull(nameof(mappingTemplates));
            _dataSourceFactory = dataSourceFactory.WhenNotNull(nameof(dataSourceFactory));
        }

        public void ConstructResolverIfRequired(Type type, PropertyInfo propertyInfo)
        {
            var attribute = propertyInfo.GetCustomAttributes(typeof(DataSourceAttribute), true).SingleOrDefault();

            if (attribute != null)
            {
                var dataSourceAttribute = attribute as DataSourceAttribute;
                var dataSource = _dataSourceFactory.CreateDataSource(dataSourceAttribute);
                var propertyName = propertyInfo.Name;

                _ = new Resolver(_graphqlApi, $"{type.Name}{propertyName}Resolver", new ResolverProps
                {
                    Api = _graphqlApi,
                    DataSource = dataSource,
                    TypeName = type.Name,
                    FieldName = propertyName.GetGraphqlName(),
                    RequestMappingTemplate = MappingTemplate.FromString(_mappingTemplates.RequestMapping),
                    ResponseMappingTemplate = MappingTemplate.FromString(_mappingTemplates.ResponseMapping)
                });
            }
        }
    }
}