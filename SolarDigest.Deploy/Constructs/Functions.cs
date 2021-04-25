using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.S3;
using System.Collections.Generic;
using AwsBucket = Amazon.CDK.AWS.S3.Bucket;

namespace SolarDigest.Deploy.Constructs
{
    internal class Functions : Construct
    {
        internal IFunction GetSiteInfoFunction { get; }
        internal IFunction HydrateAllSitesPowerFunction { get; }
        internal IFunction HydrateSitePowerFunction { get; }
        internal IFunction EmailExceptionFunction { get; }

        public Functions(Construct scope, SolarDigestApiProps apiProps, Iam iam, DynamoDbTables tables)
            : base(scope, "Functions")
        {
            var codeBucket = AwsBucket.FromBucketName(this, "CodeBucket", Constants.S3LambdaCodeBucketName);

            GetSiteInfoFunction = CreateFunction(apiProps.AppName, "GetSiteInfo", "Get site details", codeBucket,
                new[] {iam.ReadWriteDynamoDbPolicyStatement});

            HydrateAllSitesPowerFunction = CreateFunction(apiProps.AppName, "HydrateAllSitesPower", "Hydrate power data for all sites", codeBucket,
                new[] { iam.ReadWriteDynamoDbPolicyStatement });

            HydrateSitePowerFunction = CreateFunction(apiProps.AppName, "HydrateSitePower", "Hydrate power data for a specified site", codeBucket,
                new[] { iam.ReadWriteDynamoDbPolicyStatement });

            EmailExceptionFunction = CreateFunction(apiProps.AppName, "EmailException", "Sends unexpected exception reports via email", codeBucket,
                new[] { iam.ReadWriteDynamoDbPolicyStatement });

            AddEventSource(tables.ExceptionTable, EmailExceptionFunction);
        }

        private IFunction CreateFunction(string appName, string name, string description, IBucket s3Bucket,
            IEnumerable<PolicyStatement> statements = null, IDictionary<string, string> variables = null)
        {
            variables ??= new Dictionary<string, string>();
            statements ??= new List<PolicyStatement>();

            var props = new FunctionProps
            {
                FunctionName = $"{appName}_{name}",
                Description = description,
                Handler = $"SolarDigest.Api::SolarDigest.Api.Functions.{name}::InvokeAsync",
                Runtime = Runtime.DOTNET_CORE_3_1,
                MemorySize = 128,
                Timeout = Duration.Seconds(60),
                Code = new S3Code(s3Bucket, Constants.S3CodeBucketKeyName),
                Environment = variables
            };

            // Create the function and add any policy statements
            var function = new Function(this, $"{name}Function", props);

            foreach (var statement in statements)
            {
                function.AddToRolePolicy(statement);
            }

            return function;
        }

        private static void AddEventSource(ITable sourceTable, IFunction function)
        {
            var exceptionTableEventSource = new DynamoEventSource(sourceTable, new DynamoEventSourceProps
            {
                StartingPosition = StartingPosition.LATEST,
                RetryAttempts = 0
            });

            function.AddEventSource(exceptionTableEventSource);
        }
    }
}