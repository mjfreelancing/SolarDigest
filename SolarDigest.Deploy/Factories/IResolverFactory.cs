using System;
using System.Reflection;

namespace SolarDigest.Deploy.Factories
{
    internal interface IResolverFactory
    {
        void ConstructResolverIfRequired(Type type, PropertyInfo propertyInfo);
    }
}