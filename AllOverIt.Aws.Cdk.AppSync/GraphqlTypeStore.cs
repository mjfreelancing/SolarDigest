using AllOverIt.Aws.Cdk.AppSync.Extensions;
using AllOverIt.Aws.Cdk.AppSync.Factories;
using AllOverIt.Helpers;
using Amazon.CDK.AWS.AppSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SystemType = System.Type;

namespace AllOverIt.Aws.Cdk.AppSync
{
    public sealed class GraphqlTypeStore : IGraphqlTypeStore
    {
        private readonly IList<SystemType> _circularReferences = new List<SystemType>();
        private readonly IResolverFactory _resolverFactory;

        private readonly IDictionary<string, Func<bool, bool, bool, GraphqlType>> _fieldTypes = new Dictionary<string, Func<bool, bool, bool, GraphqlType>>
        {
            {nameof(Int32), (isRequired, isList, isRequiredList) => GraphqlType.Int(CreateTypeOptions(isRequired, isList, isRequiredList))},
            {nameof(Boolean), (isRequired, isList, isRequiredList) => GraphqlType.Boolean(CreateTypeOptions(isRequired, isList, isRequiredList))},
            {nameof(String), (isRequired, isList, isRequiredList) => GraphqlType.String(CreateTypeOptions(isRequired, isList, isRequiredList))},
        };

        public GraphqlTypeStore(IResolverFactory resolverFactory)
        {
            _resolverFactory = resolverFactory.WhenNotNull(nameof(resolverFactory));
        }

        // array types are supported
        public GraphqlType GetGraphqlType(SystemType type, bool isRequired, bool isList, bool isRequiredList, Action<IIntermediateType> typeCreated)
        {
            var fieldTypeCreator = GetTypeCreator(type, typeCreated);

            return fieldTypeCreator.Invoke(isRequired, isList, isRequiredList);
        }

        private Func<bool, bool, bool, GraphqlType> GetTypeCreator(SystemType type, Action<IIntermediateType> typeCreated)
        {
            var isList = type.IsArray;

            var fieldType = isList
                ? type.GetElementType()
                : type;

            var propTypeName = fieldType!.Name;

            if (!_fieldTypes.TryGetValue(propTypeName, out var fieldTypeCreator))
            {
                var objectType = CreateType(fieldType, typeCreated);

                // notify of type creation so it can, for example, be added to a schema
                typeCreated.Invoke(objectType);

                fieldTypeCreator = _fieldTypes[propTypeName];
            }

            return fieldTypeCreator;
        }

        private IIntermediateType CreateType(SystemType type, Action<IIntermediateType> typeCreated)
        {
            try
            {
                if (_circularReferences.Contains(type))
                {
                    var typeNames = string.Join(" -> ", _circularReferences.Select(item => item.Name).Concat(new[] { type.Name }));
                    throw new NotSupportedException($"Not currently supporting type circular references (type '{typeNames}')");
                }

                _circularReferences.Add(type);

                var classDefinition = new Dictionary<string, IField>();

                var isInputType = type.GetTypeInfo().IsGqlInputType();
                var properties = type.GetProperties();

                foreach (var propertyInfo in properties)
                {
                    var propertyType = propertyInfo.PropertyType;

                    if (isInputType && propertyType != typeof(string) && (propertyType.IsInterface || propertyType.IsClass))
                    {
                        if (!propertyType.GetTypeInfo().IsGqlInputType())
                        {
                            throw new InvalidOperationException($"The property '{propertyInfo.Name}' is not an INPUT type ({propertyType.Name})");
                        }
                    }

                    var isRequired = propertyInfo.IsGqlTypeRequired();
                    var isList = propertyType.IsArray;
                    var isRequiredList = isList && propertyInfo.IsGqlArrayRequired();

                    // create the field definition
                    var fieldTypeCreator = GetTypeCreator(propertyType, typeCreated);
                    classDefinition.Add(propertyInfo.Name.GetGraphqlName(), fieldTypeCreator.Invoke(isRequired, isList, isRequiredList));

                    // check if the field requires a resolver
                    _resolverFactory.ConstructResolverIfRequired(type, propertyInfo);
                }

                var intermediateType = isInputType
                    ? (IIntermediateType) new InputType(type.Name, new IntermediateTypeOptions
                    {
                        Definition = classDefinition
                    })
                    : new ObjectType(type.Name, new ObjectTypeOptions
                    {
                        Definition = classDefinition
                    });

                // cache for possible future use
                _fieldTypes.Add(
                    intermediateType.Name,
                    (isRequired, isList, isRequiredList) => intermediateType.Attribute(CreateTypeOptions(isRequired, isList, isRequiredList))
                );

                return intermediateType;

            }
            finally
            {
                _circularReferences.Remove(type);
            }
        }

        private static GraphqlTypeOptions CreateTypeOptions(bool isRequired, bool isList, bool isRequiredList)
        {
            return new GraphqlTypeOptions
            {
                IsRequired = isRequired,
                IsList = isList,
                IsRequiredList = isRequiredList
            };
        }
    }
}