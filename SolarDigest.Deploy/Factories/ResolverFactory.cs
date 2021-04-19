using AllOverIt.Helpers;
using Amazon.CDK.AWS.AppSync;
using SolarDigest.Deploy.Attributes;
using SolarDigest.Deploy.Extensions;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace SolarDigest.Deploy.Factories
{
    internal sealed class ResolverFactory : IResolverFactory
    {
        private readonly GraphqlApi _graphqlApi;
        private readonly SolarDigestApiProps _apiProps;
        private readonly IDataSourceFactory _dataSourceFactory;

        public ResolverFactory(GraphqlApi graphqlApi, SolarDigestApiProps apiProps, IDataSourceFactory dataSourceFactory)
        {
            _graphqlApi = graphqlApi.WhenNotNull(nameof(graphqlApi));
            _apiProps = apiProps.WhenNotNull(nameof(apiProps));
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
                    RequestMappingTemplate = MappingTemplate.FromString(_apiProps.MappingTemplates.RequestMapping),
                    ResponseMappingTemplate = MappingTemplate.FromString(_apiProps.MappingTemplates.ResponseMapping)
                });
            }
        }
    }
}