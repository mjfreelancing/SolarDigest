using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Helpers;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using SolarDigest.Deploy.Extensions;
using SolarDigest.Deploy.Helpers;
using AwsBucket = Amazon.CDK.AWS.S3.Bucket;

namespace SolarDigest.Deploy.Constructs
{
    internal class Functions : Construct
    {
        private readonly IMappingTemplates _mappingTemplates;
        private readonly SolarDigestAppProps _appProps;
        private readonly Iam _iam;
        private readonly IBucket _codeBucket;

        internal IFunction AddSiteFunction { get; private set; }
        internal IFunction UpdateSiteFunction { get; private set; }
        internal IFunction GetSiteFunction { get; private set; }
        internal IFunction HydrateAllSitesPowerFunction { get; private set; }
        internal IFunction HydrateSitePowerFunction { get; private set; }
        internal IFunction EmailExceptionFunction { get; private set; }
        internal IFunction AggregateAllSitesPowerFunction { get; private set; }
        internal IFunction AggregateSitePowerFunction { get; private set; }
        internal IFunction GetSitePowerSummaryFunction { get; private set; }
        internal IFunction EmailSiteUpdateHistoryFunction { get; private set; }

        public Functions(Construct scope, SolarDigestAppProps appProps, Iam iam, IMappingTemplates mappingTemplates)
            : base(scope, "Function")
        {
            _appProps = appProps.WhenNotNull(nameof(appProps));
            _iam = iam.WhenNotNull(nameof(iam));
            _mappingTemplates = mappingTemplates.WhenNotNull(nameof(mappingTemplates));

            _codeBucket = AwsBucket.FromBucketName(this, "CodeBucket", Constants.S3LambdaCodeBucketName);

            CreateGetSiteFunction();
            CreateAddSiteFunction();
            CreateUpdateSiteFunction();
            CreateEmailExceptionFunction();
            CreateHydrateAllSitesPowerFunction();
            CreateHydrateSitePowerFunction();
            CreateAggregateAllSitesPowerFunction();
            CreateAggregateSitePowerFunction();
            CreateGetSitePowerSummary();
            CreateEmailSiteUpdateHistoryFunction();
        }

        private IFunction CreateFunction(string appName, string name, string description, double? memorySize = default, int timeoutMinutes = 5)
        {
            var props = new FunctionProps
            {
                FunctionName = $"{appName}_{name}",
                Description = description,
                Handler = $"SolarDigest.Api::SolarDigest.Api.Functions.{name}::InvokeAsync",
                Runtime = Runtime.DOTNET_CORE_3_1,
                MemorySize = memorySize,
                Timeout = Duration.Minutes(timeoutMinutes),
                Code = new S3Code(_codeBucket, Constants.S3CodeBucketKeyName),
                LogRetention = RetentionDays.ONE_WEEK,
            };

            var function = new Function(this, $"{name}Function", props);

            return function;
        }

        private void CreateGetSiteFunction()
        {
            GetSiteFunction =
                CreateFunction(_appProps.AppName, Constants.Function.GetSite, "Get site details")
                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantReadTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));
        }

        private void CreateAddSiteFunction()
        {
            AddSiteFunction =
                CreateFunction(_appProps.AppName, Constants.Function.AddSite, "Add site details")
                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception))
                    .GrantReadWriteTableData(_iam, nameof(DynamoDbTables.Site));
        }

        private void CreateUpdateSiteFunction()
        {
            UpdateSiteFunction =
                CreateFunction(_appProps.AppName, Constants.Function.UpdateSite, "Update site details")
                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception))
                    .GrantReadWriteTableData(_iam, nameof(DynamoDbTables.Site));
        }

        private void CreateEmailExceptionFunction()
        {
            // Note: the EmailException function would never write to ExceptionTable as it would potentially cause an endless loop of processing

            // exceptions are forwarded via a DynamoDb stream from the Exception table to the EmailException function

            var exceptionTable = Table.FromTableAttributes(this, "FromTableAttributesException", new TableAttributes
            {
                TableArn = Fn.ImportValue(TableHelpers.GetExportTableName(nameof(DynamoDbTables.Exception))),
                TableStreamArn = Fn.ImportValue(TableHelpers.GetExportStreamName(nameof(DynamoDbTables.Exception)))
            });

            EmailExceptionFunction =
                CreateFunction(_appProps.AppName, Constants.Function.EmailException, "Sends unexpected exception reports via email")
                    .AddPolicyStatements(_iam.SendEmailPolicyStatement)
                    .AddEventSource(exceptionTable)
                    .GrantStreamReadData(_iam, nameof(DynamoDbTables.Exception));
        }

        private void CreateHydrateAllSitesPowerFunction()
        {
            HydrateAllSitesPowerFunction =
                CreateFunction(_appProps.AppName, Constants.Function.HydrateAllSitesPower, "Hydrate power data for all sites")
                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site))
                    .AddPolicyStatements(_iam.PutDefaultEventBridgeEventsPolicyStatement)
                    .GrantReadTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));
        }

        private void CreateHydrateSitePowerFunction()
        {
            HydrateSitePowerFunction =
                CreateFunction(_appProps.AppName, Constants.Function.HydrateSitePower, "Hydrate power data for a specified site", 192, 15)
                    .AddPolicyStatements(_iam.PutDefaultEventBridgeEventsPolicyStatement)
                    .AddPolicyStatements(_iam.GetDynamoDescribeTablePolicy(nameof(DynamoDbTables.Site)))
                    .AddPolicyStatements(_iam.GetDynamoBatchWriteTablePolicy(nameof(DynamoDbTables.Power)))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception), nameof(DynamoDbTables.PowerUpdateHistory))
                    .GrantReadWriteTableData(_iam, nameof(DynamoDbTables.Site));
        }

        private void CreateAggregateAllSitesPowerFunction()
        {
            AggregateAllSitesPowerFunction =
                CreateFunction(_appProps.AppName, Constants.Function.AggregateAllSitesPower, "Aggregate power data for all sites")
                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site))
                    .AddPolicyStatements(_iam.PutDefaultEventBridgeEventsPolicyStatement)
                    .GrantReadTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));
        }

        private void CreateAggregateSitePowerFunction()
        {
            AggregateSitePowerFunction =
                CreateFunction(_appProps.AppName, Constants.Function.AggregateSitePower, "Aggregate power data for a specified site", 192, 15)

                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site), nameof(DynamoDbTables.Power))

                    .AddPolicyStatements(_iam.GetDynamoQueryTablePolicy(nameof(DynamoDbTables.Power)))
                    .AddPolicyStatements(_iam.GetDynamoBatchWriteTablePolicy(nameof(DynamoDbTables.PowerMonthly), nameof(DynamoDbTables.PowerYearly)))

                    .GrantReadWriteTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));
        }

        private void CreateGetSitePowerSummary()
        {
            // YET TO BE IMPLEMENTED - ONLY ADDED TO TEST THE SCHEMA BUILDER AT THIS STAGE
            GetSitePowerSummaryFunction =
                CreateFunction(_appProps.AppName, Constants.Function.GetSitePowerSummary, "Get a power summary for a specified site", 192, 15)
                    .GrantDescribeTableData(_iam,
                        nameof(DynamoDbTables.Site), nameof(DynamoDbTables.Power),
                        nameof(DynamoDbTables.PowerMonthly), nameof(DynamoDbTables.PowerYearly))

                    .AddPolicyStatements(_iam.GetDynamoQueryTablePolicy(nameof(DynamoDbTables.Power),
                        nameof(DynamoDbTables.PowerMonthly), nameof(DynamoDbTables.PowerYearly)))

                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));

            _mappingTemplates.RegisterRequestMapping(
                Constants.Function.GetSitePowerSummary,
                StringHelpers.AppendAsLines(
                    "{",
                    @"  ""version"" : ""2017-02-28"",",
                    @"  ""operation"": ""Invoke"",",
                    @"  ""payload"": {",
                    @"    ""siteId"": $util.toJson($context.source.id),",
                    @"    ""startDate"": $util.toJson($context.arguments.filter.startDate),",
                    @"    ""endDate"": $util.toJson($context.arguments.filter.endDate),",
                    @"    ""meterType"": $util.toJson($context.arguments.filter.meterType),",
                    @"    ""summaryType"": $util.toJson($context.arguments.filter.summaryType)",
                    "  }",
                    "}"
                )
            );
        }

        private void CreateEmailSiteUpdateHistoryFunction()
        {
            EmailSiteUpdateHistoryFunction =
                CreateFunction(_appProps.AppName, Constants.Function.EmailAllSitesUpdateHistory, "Sends all sites a summary of power processing")
                    .AddPolicyStatements(_iam.SendEmailPolicyStatement)

                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site), nameof(DynamoDbTables.PowerUpdateHistory))

                    .GrantReadWriteTableData(_iam, nameof(DynamoDbTables.Site))

                    .AddPolicyStatements(_iam.GetDynamoQueryTablePolicy(nameof(DynamoDbTables.PowerUpdateHistory)))

                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));
        }
    }
}