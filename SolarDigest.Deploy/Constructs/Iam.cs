using Amazon.CDK;
using Amazon.CDK.AWS.IAM;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class Iam : Construct
    {
        //internal PolicyStatement InvokeFunctionAccessPolicyStatement { get; private set; }
        internal PolicyStatement PutEventBridgeEventsPolicyStatement { get; private set; }
        internal PolicyStatement ReadWriteDynamoDbPolicyStatement { get; private set; }

        public Iam(Construct stack, string appName)
            : base(stack, $"{appName}IAM")
        {
            //CreateInvokeFunctionAccessPolicyStatement(appName);
            CreateEventBridgePolicyStatements(appName);
            CreateReadWriteDynamoDbPolicyStatements();
        }

        //private void CreateInvokeFunctionAccessPolicyStatement(string appName)
        //{
        //    var stack = Stack.Of(this);

        //    InvokeFunctionAccessPolicyStatement = new PolicyStatement(new PolicyStatementProps
        //    {
        //        Effect = Effect.ALLOW,
        //        Actions = new[]
        //        {
        //            "lambda:InvokeFunction"
        //        },
        //        Resources = new[]
        //        {
        //            // wildcard to all functions and logs for this account / app
        //            $"arn:aws:lambda:{stack.Region}:{stack.Account}:function:{appName}DataProcedure"
        //        }
        //    });
        //}

        private void CreateEventBridgePolicyStatements(string appName)
        {
            var stack = Stack.Of(this);

            PutEventBridgeEventsPolicyStatement = new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "events:PutEvents"
                },
                Resources = new[]
                {
                    $"arn:aws:events:{stack.Region}:{stack.Account}:event-bus/{appName}"
                }
            });
        }

        private void CreateReadWriteDynamoDbPolicyStatements()
        {
            var stack = Stack.Of(this);

            ReadWriteDynamoDbPolicyStatement = new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "dynamodb:BatchGetItem",
                    "dynamodb:BatchWriteItem",
                    "dynamodb:ConditionCheckItem",
                    "dynamodb:PutItem",
                    "dynamodb:DescribeTable",
                    "dynamodb:DeleteItem",
                    "dynamodb:GetItem",
                    "dynamodb:Scan",
                    "dynamodb:Query",
                    "dynamodb:UpdateItem"
                },
                Resources = new[]
                {
                    $"arn:aws:dynamodb:{stack.Region}:{stack.Account}:table/*/",
                    $"arn:aws:dynamodb:{stack.Region}:{stack.Account}:table/*/index/"
                }
            });
        }
    }
}