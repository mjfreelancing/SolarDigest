using System;
using System.Reflection;

namespace AllOverIt.Aws.Cdk.AppSync.Factories
{
    public interface IResolverFactory
    {
        void ConstructResolverIfRequired(Type type, PropertyInfo propertyInfo);
    }
}