using AllOverIt.Aws.Cdk.AppSync.Helpers;
using AllOverIt.Aws.Cdk.AppSync.MappingTemplates;
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
        internal IFunction GetUploadUrlFunction { get; private set; }
        internal IFunction GetUploadMultiPartUrlsFunction { get; private set; }
        internal IFunction GetUploadMultiPartAbortFunction { get; private set; }
        internal IFunction GetUploadMultiPartCompleteFunction { get; private set; }
        internal IFunction GetDownloadUrlFunction { get; private set; }

        public Functions(Construct scope, Iam iam, IMappingTemplates mappingTemplates)
            : base(scope, "Function")
        {
            _iam = iam.WhenNotNull(nameof(iam));
            _mappingTemplates = mappingTemplates.WhenNotNull(nameof(mappingTemplates));

            var bucketName = $"{Shared.Helpers.GetAppVersionName()}-{Constants.S3Buckets.LambdaSourceCodeBucketName}".ToLower();
            _codeBucket = AwsBucket.FromBucketName(this, "CodeBucket", bucketName);

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
            CreateGetUploadUrlFunction();
            CreateGetUploadMultiPartUrlsFunction();
            CreateGetUploadMultiPartAbortFunction();
            CreateGetUploadMultiPartCompleteFunction();
            CreateGetDownloadUrlFunction();
        }

        private IFunction CreateFunction(string name, string description, double? memorySize = default, int timeoutMinutes = 5)
        {
            var props = new FunctionProps
            {
                FunctionName = $"{Shared.Helpers.GetAppVersionName()}_{name}",
                Description = description,
                Handler = $"SolarDigest.Api::SolarDigest.Api.Functions.{name}::InvokeAsync",
                Runtime = Runtime.DOTNET_CORE_3_1,
                MemorySize = memorySize,
                Timeout = Duration.Minutes(timeoutMinutes),
                Code = new S3Code(_codeBucket, Constants.S3CodeBucketKeyName),
                LogRetention = RetentionDays.ONE_WEEK,
            };

            return new Function(this, $"{name}Function", props);
        }

        private void CreateGetSiteFunction()
        {
            GetSiteFunction =
                CreateFunction(Constants.Function.GetSite, "Get site details")
                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantReadTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));
        }

        private void CreateAddSiteFunction()
        {
            AddSiteFunction =
                CreateFunction(Constants.Function.AddSite, "Add site details")
                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception))
                    .GrantReadWriteTableData(_iam, nameof(DynamoDbTables.Site));
        }

        private void CreateUpdateSiteFunction()
        {
            UpdateSiteFunction =
                CreateFunction(Constants.Function.UpdateSite, "Update site details")
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
                CreateFunction(Constants.Function.EmailException, "Sends unexpected exception reports via email")
                    .GrantSendEmail(_iam)
                    .AddEventSource(exceptionTable)
                    .GrantStreamReadData(_iam, nameof(DynamoDbTables.Exception));
        }

        private void CreateHydrateAllSitesPowerFunction()
        {
            HydrateAllSitesPowerFunction =
                CreateFunction(Constants.Function.HydrateAllSitesPower, "Hydrate power data for all sites")
                    .GrantPutDefaultEventBridgeEvents(_iam)
                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantReadTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));
        }

        private void CreateHydrateSitePowerFunction()
        {
            HydrateSitePowerFunction =
                CreateFunction(Constants.Function.HydrateSitePower, "Hydrate power data for a specified site", 512, 15)
                    .GrantPutDefaultEventBridgeEvents(_iam)
                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantBatchWriteTableData(_iam, nameof(DynamoDbTables.Power))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception), nameof(DynamoDbTables.PowerUpdateHistory))
                    .GrantReadWriteTableData(_iam, nameof(DynamoDbTables.Site));
        }

        private void CreateAggregateAllSitesPowerFunction()
        {
            AggregateAllSitesPowerFunction =
                CreateFunction(Constants.Function.AggregateAllSitesPower, "Aggregate power data for all sites")
                    .GrantPutDefaultEventBridgeEvents(_iam)
                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantReadTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));
        }

        private void CreateAggregateSitePowerFunction()
        {
            AggregateSitePowerFunction =
                CreateFunction(Constants.Function.AggregateSitePower, "Aggregate power data for a specified site", 512)
                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site), nameof(DynamoDbTables.Power))
                    .GrantQueryTableData(_iam, nameof(DynamoDbTables.Power))
                    .GrantBatchWriteTableData(_iam, nameof(DynamoDbTables.PowerMonthly), nameof(DynamoDbTables.PowerYearly))
                    .GrantReadWriteTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));
        }

        private void CreateGetSitePowerSummary()
        {
            // Testing shows 768MB RAM is required to avoid DynamoDb execution timeout even though the function only uses around 140MB
            GetSitePowerSummaryFunction =
                CreateFunction(Constants.Function.GetSitePowerSummary, "Get a power summary for a specified site", 768)
                    .GrantDescribeTableData(_iam,
                        nameof(DynamoDbTables.Site), nameof(DynamoDbTables.Power),
                        nameof(DynamoDbTables.PowerMonthly), nameof(DynamoDbTables.PowerYearly))

                    .GrantQueryTableData(_iam, nameof(DynamoDbTables.Power), nameof(DynamoDbTables.PowerMonthly),
                        nameof(DynamoDbTables.PowerYearly))

                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));

            _mappingTemplates.RegisterRequestMapping(
                Constants.Function.GetSitePowerSummary,
                StringHelpers.Prettify(
                    @"
                    {
                      ""version"" : ""2017-02-28"",
                      ""operation"": ""Invoke"",
                      ""payload"": {
                        ""siteId"": $util.toJson($ctx.source.id),

                        #if (!$util.isNull($ctx.args.limit))
                          ""limit"": $util.toJson($ctx.args.limit),
                        #end

                        #if (!$util.isNull($ctx.args.startCursor))
                          ""startCursor"": $util.toJson($ctx.args.startCursor),
                        #end

                        ""startDate"": $util.toJson($ctx.args.filter.startDate),
                        ""endDate"": $util.toJson($ctx.args.filter.endDate),
                        ""meterType"": $util.toJson($ctx.args.filter.meterType),
                        ""summaryType"": $util.toJson($ctx.args.filter.summaryType)
                      }
                    }"
                )
            );
        }

        private void CreateEmailSiteUpdateHistoryFunction()
        {
            EmailSiteUpdateHistoryFunction =
                CreateFunction(Constants.Function.EmailAllSitesUpdateHistory, "Sends all sites a summary of power processing")
                    .GrantSendEmail(_iam)
                    .GrantDescribeTableData(_iam, nameof(DynamoDbTables.Site), nameof(DynamoDbTables.PowerUpdateHistory))
                    .GrantReadWriteTableData(_iam, nameof(DynamoDbTables.Site))
                    .GrantQueryTableData(_iam, nameof(DynamoDbTables.PowerUpdateHistory))
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));
        }

        private void CreateGetUploadUrlFunction()
        {
            GetUploadUrlFunction =
                CreateFunction(Constants.Function.GetUploadUrl, "Generates a pre-signed Url that allows a file to be uploaded")
                    .GrantPutParameters(_iam, "Secrets")
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));

            _mappingTemplates.RegisterRequestMapping(
                Constants.Function.GetUploadUrl,
                StringHelpers.Prettify(
                    @"
                    {
                      ""version"" : ""2017-02-28"",
                      ""operation"": ""Invoke"",
                      ""payload"": {
                        ""filename"": $util.toJson($ctx.args.input.filename),
                        ""uploadId"": $util.toJson($ctx.args.input.uploadId),

                        #if (!$util.isNull($ctx.args.input.partNumber))
                          ""partNumber"": $util.toJson($ctx.args.input.partNumber),
                        #end
                      }
                    }"
                )
            );
        }

        private void CreateGetUploadMultiPartUrlsFunction()
        {
            GetUploadMultiPartUrlsFunction =
                CreateFunction(Constants.Function.GetUploadMultiPartUrls, "Generates pre-signed Urls for a batch of uploads")
                    .GrantPutParameters(_iam, "Secrets")
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));

            _mappingTemplates.RegisterRequestMapping(
                Constants.Function.GetUploadMultiPartUrls,
                StringHelpers.Prettify(
                    @"
                    {
                      ""version"" : ""2017-02-28"",
                      ""operation"": ""Invoke"",
                      ""payload"": {
                        ""filename"": $util.toJson($ctx.args.input.filename),
                        ""partCount"": $util.toJson($ctx.args.input.partCount)
                      }
                    }"
                )
            );
        }

        private void CreateGetUploadMultiPartAbortFunction()
        {
            GetUploadMultiPartAbortFunction =
                CreateFunction(Constants.Function.GetUploadMultiPartAbort, "Aborts a previously initiated multi-part upload")
                    .GrantPutParameters(_iam, "Secrets")
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));

            _mappingTemplates.RegisterRequestMapping(
                Constants.Function.GetUploadMultiPartAbort,
                StringHelpers.Prettify(
                    @"
                    {
                      ""version"" : ""2017-02-28"",
                      ""operation"": ""Invoke"",
                      ""payload"": {
                        ""filename"": $util.toJson($ctx.args.input.filename),
                        ""uploadId"": $util.toJson($ctx.args.input.uploadId)
                      }
                    }"
                )
            );
        }

        private void CreateGetUploadMultiPartCompleteFunction()
        {
            GetUploadMultiPartCompleteFunction =
                CreateFunction(Constants.Function.GetUploadMultiPartComplete, "Completes a previously initiated multi-part upload")
                    .GrantPutParameters(_iam, "Secrets")
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));

            _mappingTemplates.RegisterRequestMapping(
                Constants.Function.GetUploadMultiPartComplete,
                StringHelpers.Prettify(
                    @"
                    {
                      ""version"" : ""2017-02-28"",
                      ""operation"": ""Invoke"",
                      ""payload"": {
                        ""filename"": $util.toJson($ctx.args.input.filename),
                        ""uploadId"": $util.toJson($ctx.args.input.uploadId),
                        ""eTags"": $util.toJson($ctx.args.input.eTags)
                      }
                    }"
                )
            );
        }

        private void CreateGetDownloadUrlFunction()
        {
            GetDownloadUrlFunction =
                CreateFunction(Constants.Function.GetDownloadUrl, "Generates a pre-signed Url that allows a file to be downloaded")
                    .GrantGetParameters(_iam, "Secrets")
                    .GrantWriteTableData(_iam, nameof(DynamoDbTables.Exception));
        }
    }
}