using SolarDigest.Deploy.Attributes;
using System.Reflection;

namespace SolarDigest.Deploy.Extensions
{
    internal static class ParameterInfoExtensions
    {
        public static bool IsGqlTypeRequired(this ParameterInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute(typeof(GraphqlTypeRequiredAttribute), true) != null;
        }

        public static bool IsGqlArrayRequired(this ParameterInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute(typeof(GraphqlArrayRequiredAttribute), true) != null;
        }
    }
}