using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Aws.Cdk.AppSync.Extensions;
using AllOverIt.Aws.Cdk.AppSync.Factories;
using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Amazon.CDK.AWS.AppSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AllOverIt.Aws.Cdk.AppSync.Schema
{
    public sealed class SchemaBuilder : ISchemaBuilder
    {
        private readonly GraphqlApi _graphQLApi;
        private readonly IMappingTemplates _mappingTemplates;
        private readonly IGraphqlTypeStore _typeStore;
        private readonly IDataSourceFactory _dataSourceFactory;

        public SchemaBuilder(GraphqlApi graphQlApi, IMappingTemplates mappingTemplates, IGraphqlTypeStore typeStore, IDataSourceFactory dataSourceFactory)
        {
            _graphQLApi = graphQlApi.WhenNotNull(nameof(graphQlApi));
            _mappingTemplates = mappingTemplates.WhenNotNull(nameof(mappingTemplates));
            _typeStore = typeStore.WhenNotNull(nameof(typeStore));
            _dataSourceFactory = dataSourceFactory.WhenNotNull(nameof(dataSourceFactory));
        }

        public ISchemaBuilder AddQuery<TType>()
            where TType : IQueryDefinition
        {
            CreateGraphqlSchemaType<TType>((fieldName, field) => _graphQLApi.AddQuery(fieldName, field));
            return this;
        }

        public ISchemaBuilder AddMutation<TType>() where TType : IMutationDefinition
        {
            CreateGraphqlSchemaType<TType>((fieldName, field) => _graphQLApi.AddMutation(fieldName, field));
            return this;
        }

        public ISchemaBuilder AddSubscription<TType>() where TType : ISubscriptionDefinition
        {
            var schemaType = typeof(TType);

            var methods = schemaType.GetMethodInfo();

            foreach (var methodInfo in methods)
            {
                var isRequired = methodInfo.IsGqlTypeRequired();
                var isList = methodInfo.ReturnType.IsArray;
                var isRequiredList = isList && methodInfo.IsGqlArrayRequired();

                var returnObjectType = _typeStore
                    .GetGraphqlType(
                        methodInfo.ReturnType,
                        isRequired,
                        isList,
                        isRequiredList,
                        objectType => _graphQLApi.AddType(objectType));

                _graphQLApi.AddSubscription(methodInfo.Name.GetGraphqlName(),
                    new ResolvableField(
                        new ResolvableFieldOptions
                        {
                            Directives = new[]
                            {
                                Directive.Subscribe(GetSubscriptionMutations(methodInfo).ToArray())
                            },
                            Args = GetMethodArgs(methodInfo),
                            ReturnType = returnObjectType
                            //ReturnType = GraphqlType.Intermediate(new GraphqlTypeOptions
                            //{
                            //    IntermediateType = returnObjectType.IntermediateType,
                            //    IsRequired = returnObjectType.IsRequired,
                            //    IsList = returnObjectType.IsList,
                            //    IsRequiredList = returnObjectType.IsRequiredList
                            //})
                        })
                );
            }

            return this;
        }

        private void CreateGraphqlSchemaType<TType>(Action<string, ResolvableField> graphqlAction)
        {
            var schemaType = typeof(TType);

            var methods = schemaType.GetMethodInfo();

            if (schemaType.IsInterface)
            {
                var inheritedProperties = schemaType.GetInterfaces().SelectMany(item => item.GetMethods());
                methods = methods.Concat(inheritedProperties);
            }

            foreach (var methodInfo in methods)
            {
                var dataSource = GetMethodDataSource(methodInfo);           // optionally specified via a custom attribute

                var isRequired = methodInfo.IsGqlTypeRequired();
                var isList = methodInfo.ReturnType.IsArray;
                var isRequiredList = isList && methodInfo.IsGqlArrayRequired();

                var returnObjectType = _typeStore
                    .GetGraphqlType(
                        methodInfo.ReturnType,
                        isRequired,
                        isList,
                        isRequiredList,
                        objectType => _graphQLApi.AddType(objectType));

                graphqlAction.Invoke(methodInfo.Name.GetGraphqlName(),
                    new ResolvableField(
                        new ResolvableFieldOptions
                        {
                            DataSource = dataSource,
                            RequestMappingTemplate = MappingTemplate.FromString(_mappingTemplates.RequestMapping),
                            ResponseMappingTemplate = MappingTemplate.FromString(_mappingTemplates.ResponseMapping),
                            Args = GetMethodArgs(methodInfo),
                            ReturnType = returnObjectType
                        })
                );
            }
        }

        private IDictionary<string, GraphqlType> GetMethodArgs(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();

            if (!parameters.Any())
            {
                return null;
            }

            var args = new Dictionary<string, GraphqlType>();

            foreach (var parameterInfo in parameters)
            {
                var paramType = parameterInfo.ParameterType;
                var isRequired = parameterInfo.IsGqlTypeRequired();
                var isList = paramType.IsArray;
                var isRequiredList = isList && parameterInfo.IsGqlArrayRequired();

                var graphQlType = _typeStore.GetGraphqlType(paramType, isRequired, isList, isRequiredList, objectType => _graphQLApi.AddType(objectType));

                args.Add(parameterInfo.Name.GetGraphqlName(), graphQlType);
            }

            return args;
        }

        private BaseDataSource GetMethodDataSource(MethodInfo methodInfo)
        {
            var attribute = methodInfo.GetCustomAttributes(typeof(DataSourceAttribute), true).SingleOrDefault();

            return attribute == null 
                ? null 
                : _dataSourceFactory.CreateDataSource(attribute as DataSourceAttribute);
        }

        private static IEnumerable<string> GetSubscriptionMutations(MethodInfo methodInfo)
        {
            var attribute = methodInfo.GetCustomAttributes(typeof(SubscriptionMutationAttribute), true).SingleOrDefault();

            return attribute == null
                ? Enumerable.Empty<string>()
                : (attribute as SubscriptionMutationAttribute)!.Mutations;
        }
    }
}