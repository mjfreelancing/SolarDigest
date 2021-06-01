using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Aws.Cdk.AppSync.Exceptions;
using System.Reflection;
using SystemType = System.Type;

namespace AllOverIt.Aws.Cdk.AppSync.Extensions
{
    internal static class TypeExtensions
    {
        public static GraphQlSchemaTypeDescriptor GetGraphqlTypeDescriptor(this SystemType type)
        {
            var typeInfo = type.GetTypeInfo();
            var inputType = typeInfo.GetCustomAttribute(typeof(GraphqlSchemaTypeAttribute), true);

            if (inputType != null)
            {
                var typeAttribute = inputType as GraphqlSchemaTypeAttribute;

                return new GraphQlSchemaTypeDescriptor(type, typeAttribute!.GraphqlSchemaType, typeAttribute.Name ?? typeInfo.Name);
            }

            if (type != typeof(string) && (type.IsClass || type.IsInterface))
            {
                throw new GraphqlSchemaException($"A class or interface based schema type must have a {nameof(GraphqlSchemaTypeAttribute)} applied ({typeInfo.Name})");
            }

            return new GraphQlSchemaTypeDescriptor(type, GraphqlSchemaType.Primitive, typeInfo.Name);
        }
    }
}