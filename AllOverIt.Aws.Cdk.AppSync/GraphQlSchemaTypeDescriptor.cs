﻿using AllOverIt.Helpers;
using SystemType = System.Type;

namespace AllOverIt.Aws.Cdk.AppSync
{
    public sealed class GraphqlSchemaTypeDescriptor
    {
        public SystemType Type { get; }
        public GraphqlSchemaType SchemaType { get; }
        public string Name { get; }

        public GraphqlSchemaTypeDescriptor(SystemType type, GraphqlSchemaType schemaType, string name)
        {
            Type = type.WhenNotNull(nameof(type));
            SchemaType = schemaType;
            Name = name.WhenNotNullOrEmpty(nameof(name));
        }
    }
}