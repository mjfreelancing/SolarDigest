using AllOverIt.Helpers;
using System;

namespace AllOverIt.Aws.Cdk.AppSync
{
    public sealed class GraphQlSchemaTypeDescriptor
    {
        public Type Type { get; }
        public GraphqlSchemaType GraphqlSchemaType { get; }
        public string Name { get; }

        public GraphQlSchemaTypeDescriptor(Type type, GraphqlSchemaType graphqlSchemaType, string name)
        {
            Type = type.WhenNotNull(nameof(type));
            GraphqlSchemaType = graphqlSchemaType;
            Name = name.WhenNotNullOrEmpty(nameof(name));
        }
    }
}