using SolarDigest.Deploy.Attributes;
using System.Reflection;

namespace SolarDigest.Deploy.Extensions
{
    internal static class TypeInfoExtensions
    {
        public static bool IsGqlInputType(this TypeInfo typeInfo)
        {
            return typeInfo.GetCustomAttribute(typeof(GraphqlInputAttribute), true) != null;
        }
    }
}