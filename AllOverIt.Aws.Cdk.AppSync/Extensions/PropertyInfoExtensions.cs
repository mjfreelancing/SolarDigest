using System.Reflection;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace AllOverIt.Aws.Cdk.AppSync.Extensions
{
    internal static class PropertyInfoExtensions
    {
        public static bool IsGqlTypeRequired(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute(typeof(GraphqlTypeRequiredAttribute), true) != null;
        }

        public static bool IsGqlArrayRequired(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute(typeof(GraphqlArrayRequiredAttribute), true) != null;
        }
    }
}