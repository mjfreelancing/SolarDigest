using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;

namespace SolarDigest.Deploy.Extensions
{
    public static class FunctionExtensions
    {
        public static void AddPolicyStatements(this IFunction function, params PolicyStatement[] statements)
        {
            foreach (var statement in statements)
            {
                function.AddToRolePolicy(statement);
            }
        }
    }
}