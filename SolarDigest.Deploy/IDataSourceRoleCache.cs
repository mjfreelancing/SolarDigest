using Amazon.CDK.AWS.IAM;

namespace SolarDigest.Deploy
{
    internal interface IDataSourceRoleCache
    {
        void AddRole(IRole role, string serviceName, params string[] functionNames);
        IRole GetRole(string serviceName, string functionName);
    }
}