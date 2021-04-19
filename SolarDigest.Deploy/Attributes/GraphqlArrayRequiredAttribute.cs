using System;

namespace SolarDigest.Deploy.Attributes
{
    // Used to indicate an array 'type' or 'input' is required
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter)]
    internal sealed class GraphqlArrayRequiredAttribute : Attribute
    {
    }
}