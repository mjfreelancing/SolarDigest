using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using SolarDigest.Deploy.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class Iam : Construct
    {
        private readonly SolarDigestAppProps _appProps;

        internal PolicyStatement PutDefaultEventBridgeEventsPolicyStatement { get; private set; }
        //internal PolicyStatement GetParameterPolicyStatement { get; private set; }
        internal PolicyStatement SendEmailPolicyStatement { get; private set; }

        public Iam(Construct scope, SolarDigestAppProps appProps)
            : base(scope, "IAM")
        {
            _appProps = appProps;
            CreateDefaultEventBridgePolicyStatement();
            //CreateGetParameterPolicyStatement();
            CreateSendEmailPolicyStatements();
        }

        public PolicyStatement GetDownloadS3Policy(string bucket)
        {
            return new(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "s3:GetObject"
                },
                Resources = new[] { $"arn:aws:s3:::{bucket}/*" }
            });
        }

        public PolicyStatement GetUploadS3Policy(string bucket)
        {
            return new(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "s3:PutObject"
                },
                Resources = new[] { $"arn:aws:s3:::{bucket}/*" }
            });
        }

        public PolicyStatement GetDynamoDescribeTablePolicy(params string[] tableNames)
        {
            return new (new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "dynamodb:DescribeTable"
                },
                Resources = GetTableArns(tableNames)
            });
        }

        public PolicyStatement GetDynamoQueryTablePolicy(params string[] tableNames)
        {
            return new (new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "dynamodb:Query"
                },
                Resources = GetTableArns(tableNames)
            });
        }

        public PolicyStatement GetDynamoBatchWriteTablePolicy(params string[] tableNames)
        {
            return new (new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "dynamodb:BatchWriteItem"
                },
                Resources = GetTableArns(tableNames)
            });
        }

        public PolicyStatement GetDynamoReadDataPolicy(params string[] tableNames)
        {
            return new (new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "dynamodb:BatchGetItem",
                    "dynamodb:GetRecords", 
                    "dynamodb:GetShardIterator",
                    "dynamodb:Query",
                    "dynamodb:GetItem",
                    "dynamodb:Scan"
                },
                Resources = GetTableArns(tableNames)
            });
        }

        public PolicyStatement GetDynamoWriteDataPolicy(params string[] tableNames)
        {
            return new (new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "dynamodb:BatchWriteItem",
                    "dynamodb:PutItem",
                    "dynamodb:UpdateItem",
                    "dynamodb:DeleteItem"
                },
                Resources = GetTableArns(tableNames)
            });
        }

        public PolicyStatement GetDynamoStreamReadPolicy(params string[] tableNames)
        {
            return new (new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "dynamodb:DescribeStream",
                    "dynamodb:GetRecords",
                    "dynamodb:GetShardIterator",
                    "dynamodb:ListStreams"
                },
                Resources = GetTableArns(tableNames)
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

        //private void CreateGetParameterPolicyStatement()
        //{
        //    var stack = Stack.Of(this);

        //    GetParameterPolicyStatement = new PolicyStatement(new PolicyStatementProps
        //    {
        //        Effect = Effect.ALLOW,
        //        Actions = new[]
        //        {
        //            "ssm:GetParameter"
        //        },
        //        Resources = new[]
        //        {
        //            $"arn:aws:ssm:{stack.Region}:{stack.Account}:parameter/*"
        //        }
        //    });
        //}

        private void CreateSendEmailPolicyStatements()
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

        private static string[] GetTableArns(params string[] tableNames)
        {
            static string GetTableArn(string tableName)
            {
                return Fn.ImportValue(TableHelpers.GetExportTableName(tableName));
            }

            return tableNames.Select(GetTableArn).ToArray();
        }
    }
}