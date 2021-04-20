using System;

namespace AllOverIt.Aws.Cdk.AppSync.Attributes
{
    // Used to indicate a schema 'input' type
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public sealed class GraphqlInputAttribute : Attribute
    {
    }
}