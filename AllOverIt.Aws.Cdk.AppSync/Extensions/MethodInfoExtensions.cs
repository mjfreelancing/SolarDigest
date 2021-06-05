using AllOverIt.Aws.Cdk.AppSync.Attributes;
using Amazon.CDK.AWS.AppSync;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AllOverIt.Aws.Cdk.AppSync.Extensions
{
    internal static class MethodInfoExtensions
    {
        public static bool IsGqlTypeRequired(this MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttribute<SchemaTypeRequiredAttribute>(true) != null;
        }

        public static bool IsGqlArrayRequired(this MethodInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<SchemaArrayRequiredAttribute>(true) != null;
        }

        public static IDictionary<string, GraphqlType> GetMethodArgs(this MethodInfo methodInfo, GraphqlApi graphqlApi, IGraphqlTypeStore typeStore)
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

                var graphqlType = typeStore.GetGraphqlType(paramType, parameterInfo, isRequired, isList, isRequiredList,
                    objectType => graphqlApi.AddType(objectType));

                args.Add(parameterInfo.Name.GetGraphqlName(), graphqlType);
            }

            return args;
        }
    }
}