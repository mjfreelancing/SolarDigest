using SolarDigest.Deploy.Attributes;
using System.Reflection;

namespace SolarDigest.Deploy.Extensions
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