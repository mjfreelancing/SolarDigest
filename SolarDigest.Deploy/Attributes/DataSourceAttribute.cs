using System;

namespace SolarDigest.Deploy.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    internal abstract class DataSourceAttribute : Attribute
    {
        // used for lookup in the DataSourceFactory
        public abstract string LookupKey { get; }
    }
}