using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;

namespace SolarDigest.Deploy.Extensions
{
    public static class FunctionExtensions
    {
        public static IFunction AddPolicyStatements(this IFunction function, params PolicyStatement[] statements)
        {
            foreach (var statement in statements)
            {
                function.AddToRolePolicy(statement);
            }

            return function;
        }
    }
}