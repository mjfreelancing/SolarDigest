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
            return methodInfo.GetCustomAttribute(typeof(GraphqlTypeRequiredAttribute), true) != null;
        }

        public static bool IsGqlArrayRequired(this MethodInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute(typeof(GraphqlArrayRequiredAttribute), true) != null;
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

                var graphQlType = typeStore.GetGraphqlType(paramType, isRequired, isList, isRequiredList, objectType => graphqlApi.AddType(objectType));

                args.Add(parameterInfo.Name.GetGraphqlName(), graphQlType);
            }

            return args;
        }
    }
}