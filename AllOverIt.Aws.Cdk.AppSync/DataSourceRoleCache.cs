using Amazon.CDK.AWS.IAM;
using System;
using System.Collections.Generic;

namespace AllOverIt.Aws.Cdk.AppSync
{
    public sealed class DataSourceRoleCache : IDataSourceRoleCache
    {
        private readonly IDictionary<string, IRole> _roles = new Dictionary<string, IRole>();

        public void AddRole(IRole role, string serviceName, params string[] functionNames)
        {
            foreach (var dataSourceName in functionNames)
            {
                var lookupKey = GetLookupKey(serviceName, dataSourceName);

                _roles.Add(lookupKey, role);
            }
        }

        public IRole GetRole(string serviceName, string functionName)
        {
            var lookupKey = GetLookupKey(serviceName, functionName);

            if (!_roles.TryGetValue(lookupKey, out var role))
            {
                throw new ArgumentOutOfRangeException(nameof(functionName), $"The DataSource Role '{functionName}' is not registered");
            }

            return role;
        }

        private static string GetLookupKey(string serviceName, string dataSourceName)
        {
            return $"{serviceName}{dataSourceName}";
        }
    }
}