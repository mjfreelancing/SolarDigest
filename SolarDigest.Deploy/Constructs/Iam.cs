using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using System.Collections.Generic;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class Iam : Construct
    {
        internal PolicyStatement PutDefaultEventBridgeEventsPolicyStatement { get; private set; }
        internal PolicyStatement GetParameterPolicyStatement { get; private set; }
        internal PolicyStatement SendEmailPolicyStatement { get; private set; }

        public Iam(Construct stack, string appName)
            : base(stack, $"{appName}IAM")
        {
            CreateDefaultEventBridgePolicyStatement();
            CreateGetParameterPolicyStatement();
            SendEmailPolicyStatements();
        }

        public PolicyStatement GetDynamoDescribeTablePolicy(string tableName)
        {
            var stack = Stack.Of(this);

            return new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "dynamodb:DescribeTable"
                },
                Resources = new[]
                {
                    $"arn:aws:dynamodb:{stack.Region}:{stack.Account}:table/{tableName}"
                }
            });
        }

        public PolicyStatement GetDynamoBatchWriteTablePolicy(string tableName)
        {
            var stack = Stack.Of(this);

            return new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "dynamodb:BatchWriteItem"
                },
                Resources = new[]
                {
                    $"arn:aws:dynamodb:{stack.Region}:{stack.Account}:table/{tableName}"
                }
            });
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
                    $"arn:aws:ssm:{stack.Region}:{stack.Account}:parameter/*"
                }
            });
        }

        private void SendEmailPolicyStatements()
        {
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
                },
                Conditions = new Dictionary<string, object>
                {
                    {
                        "ForAllValues:StringLike",
                        new Dictionary<string, object>
                        {
                            {"ses:Recipients", "*@mjfreelancing.com"},
                            {"ses:FromAddress", "*@mjfreelancing.com"}
                        }
                    }
                }
            });
        }
    }
}