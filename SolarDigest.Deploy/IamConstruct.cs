using Amazon.CDK;
using Amazon.CDK.AWS.IAM;

namespace SolarDigest.Deploy
{
    internal sealed class IamConstruct : Construct
    {
        internal PolicyStatement InvokeFunctionAccessPolicyStatement { get; }

        public IamConstruct(Construct stack, string appName)
            : base(stack, $"{appName}IAM")
        {
            InvokeFunctionAccessPolicyStatement = CreateInvokeFunctionAccessPolicyStatement(appName);
        }

        private PolicyStatement CreateInvokeFunctionAccessPolicyStatement(string appName)
        {
            var stack = Stack.Of(this);

            return new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "lambda:InvokeFunction"
                },
                Resources = new[]
                {
                    // wildcard to all functions and logs for this account / app
                    $"arn:aws:lambda:{stack.Region}:{stack.Account}:function:{appName}DataProcedure"
                }
            });
        }
    }
}