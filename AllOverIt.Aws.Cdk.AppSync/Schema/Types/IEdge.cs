﻿using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace AllOverIt.Aws.Cdk.AppSync.Schema.Types
{
    public interface IEdge<out TNode>
    {
        [SchemaTypeRequired]
        public TNode Node { get; }

        [SchemaTypeRequired]
        public string Cursor { get; }
    }
}