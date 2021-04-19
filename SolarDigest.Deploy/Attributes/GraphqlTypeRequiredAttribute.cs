using System;

namespace SolarDigest.Deploy.Attributes
{
    // Used to indicate a schema scalar, custom 'type', or 'input' is required
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter)]
    internal sealed class GraphqlTypeRequiredAttribute : Attribute
    {
    }
}