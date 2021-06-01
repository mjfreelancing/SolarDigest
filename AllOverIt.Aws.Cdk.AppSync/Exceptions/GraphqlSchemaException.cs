using System;

namespace AllOverIt.Aws.Cdk.AppSync.Exceptions
{
    public sealed class GraphqlSchemaException : Exception
    {
        public GraphqlSchemaException(string message)
            : base(message)
        {
        }
    }
}