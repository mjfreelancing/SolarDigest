using Amazon.CDK;
using Amazon.CDK.AWS.IAM;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class Iam : Construct
    {
        //internal PolicyStatement InvokeFunctionAccessPolicyStatement { get; private set; }
        internal PolicyStatement PutEventBridgeEventsPolicyStatement { get; private set; }
        internal PolicyStatement GetParameterPolicyStatement { get; private set; }                      // todo: attached to the function that needs to use GetApiKey()
        //internal PolicyStatement ReadDynamoDbPolicyStatement { get; private set; }
        //internal PolicyStatement WriteDynamoDbPolicyStatement { get; private set; }
        //internal PolicyStatement ReadDynamoDbStreamPolicyStatement { get; private set; }
        internal PolicyStatement SendEmailPolicyStatement { get; private set; }

        public Iam(Construct stack, string appName)
            : base(stack, $"{appName}IAM")
        {
            //CreateInvokeFunctionAccessPolicyStatement(appName);
            CreateEventBridgePolicyStatements(appName);
            CreateGetParameterPolicyStatements(appName);
            //CreateReadWriteDynamoDbPolicyStatements();
            //CreateReadDynamoDbStreamPolicyStatements();
            SendEmailPolicyStatements();
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
                    $"arn:aws:events:{stack.Region}:{stack.Account}:event-bus/default"
                }
            });
        }

        private void CreateGetParameterPolicyStatements(string appName)
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

        //private void CreateReadWriteDynamoDbPolicyStatements()
        //{
        //    var stack = Stack.Of(this);

        //    ReadDynamoDbPolicyStatement = new PolicyStatement(new PolicyStatementProps
        //    {
        //        Effect = Effect.ALLOW,
        //        Actions = new[]
        //        {
        //            "dynamodb:BatchGetItem",
        //            "dynamodb:ConditionCheckItem",
        //            "dynamodb:DescribeTable",
        //            "dynamodb:GetItem",
        //            "dynamodb:Scan",
        //            "dynamodb:Query",
        //        },
        //        Resources = new[]
        //        {
        //            $"arn:aws:dynamodb:{stack.Region}:{stack.Account}:table/*/",
        //            $"arn:aws:dynamodb:{stack.Region}:{stack.Account}:table/*/index/"
        //        }
        //    });

        //    WriteDynamoDbPolicyStatement = new PolicyStatement(new PolicyStatementProps
        //    {
        //        Effect = Effect.ALLOW,
        //        Actions = new[]
        //        {
        //            "dynamodb:BatchWriteItem",
        //            "dynamodb:PutItem",
        //            "dynamodb:DescribeTable",
        //            "dynamodb:DeleteItem",
        //            "dynamodb:UpdateItem"
        //        },
        //        Resources = new[]
        //        {
        //            $"arn:aws:dynamodb:{stack.Region}:{stack.Account}:table/*/",
        //            $"arn:aws:dynamodb:{stack.Region}:{stack.Account}:table/*/index/"
        //        }
        //    });
        //}

        //private void CreateReadDynamoDbStreamPolicyStatements()
        //{
        //    var stack = Stack.Of(this);

        //    ReadDynamoDbStreamPolicyStatement = new PolicyStatement(new PolicyStatementProps
        //    {
        //        Effect = Effect.ALLOW,
        //        Actions = new[]
        //        {
        //            "dynamodb:DescribeStream",
        //            "dynamodb:GetRecords",
        //            "dynamodb:GetShardIterator",
        //            "dynamodb:ListStreams"
        //        },
        //        Resources = new[]
        //        {
        //            $"arn:aws:dynamodb:{stack.Region}:{stack.Account}:table/Exception/stream/*",
        //        }
        //    });
        //}

        private void SendEmailPolicyStatements()
        {
            var stack = Stack.Of(this);

            SendEmailPolicyStatement = new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "ses:SendEmail"
                    //"ses:SendRawEmail"
                },
                Resources = new[]
                {
                    "*"      //$"arn:aws:ses:{stack.Region}:{stack.Account}:identity/*",
                }
            });
        }
    }
}