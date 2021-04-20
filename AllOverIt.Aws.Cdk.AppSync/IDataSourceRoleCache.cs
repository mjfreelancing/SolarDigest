using Amazon.CDK.AWS.IAM;

namespace AllOverIt.Aws.Cdk.AppSync
{
    public interface IDataSourceRoleCache
    {
        void AddRole(IRole role, string serviceName, params string[] functionNames);
        IRole GetRole(string serviceName, string functionName);
    }
}