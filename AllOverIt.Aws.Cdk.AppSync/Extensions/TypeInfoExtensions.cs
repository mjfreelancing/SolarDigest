using System.Reflection;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace AllOverIt.Aws.Cdk.AppSync.Extensions
{
    internal static class TypeInfoExtensions
    {
        public static bool IsGqlInputType(this TypeInfo typeInfo)
        {
            return typeInfo.GetCustomAttribute(typeof(GraphqlInputAttribute), true) != null;
        }
    }
}