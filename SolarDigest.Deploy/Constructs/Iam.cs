using Amazon.CDK;
using Amazon.CDK.AWS.IAM;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class Iam : Construct
    {
        internal PolicyStatement PutDefaultEventBridgeEventsPolicyStatement { get; private set; }
        internal PolicyStatement GetParameterPolicyStatement { get; private set; }
        internal PolicyStatement DynamoDescribeTablePolicy { get; private set; }
        internal PolicyStatement SendEmailPolicyStatement { get; private set; }

        public Iam(Construct stack, string appName)
            : base(stack, $"{appName}IAM")
        {
            CreateDefaultEventBridgePolicyStatement();
            CreateGetParameterPolicyStatement();
            CreateDynamoDescribeTablePolicyStatement();
            SendEmailPolicyStatements();
        }

        private void CreateDefaultEventBridgePolicyStatement()
        {
            var stack = Stack.Of(this);

            PutDefaultEventBridgeEventsPolicyStatement = new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "events:PutEvents"
                },
                Resources = new[]
                {
                    $"arn:aws:events:{stack.Region}:{stack.Account}:event-bus/default"
                }
            });
        }

        private void CreateGetParameterPolicyStatement()
        {
            var stack = Stack.Of(this);

            GetParameterPolicyStatement = new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "ssm:GetParameter"
                },
                Resources = new[]
                {
                    $"arn:aws:events:{stack.Region}:{stack.Account}:parameter/*"
                }
            });
        }

        private void CreateDynamoDescribeTablePolicyStatement()
        {
            var stack = Stack.Of(this);

            DynamoDescribeTablePolicy = new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "dynamodb:DescribeTable"
                },
                Resources = new[]
                {
                    $"arn:aws:events:{stack.Region}:{stack.Account}:table/*"
                }
            });
        }

        private void SendEmailPolicyStatements()
        {
            var stack = Stack.Of(this);

            SendEmailPolicyStatement = new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "ses:SendEmail"
                },
                Resources = new[]
                {
                    "*"      //$"arn:aws:ses:{stack.Region}:{stack.Account}:identity/*",
                }
            });
        }
    }
}