using System;

namespace SolarDigest.Deploy.Attributes
{
    // Used to indicate a schema 'input' type
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    internal sealed class GraphqlInputAttribute : Attribute
    {
    }
}