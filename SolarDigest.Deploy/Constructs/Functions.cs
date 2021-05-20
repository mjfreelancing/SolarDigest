﻿using AllOverIt.Helpers;
using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using SolarDigest.Deploy.Extensions;
using AwsBucket = Amazon.CDK.AWS.S3.Bucket;

namespace SolarDigest.Deploy.Constructs
{
    internal class Functions : Construct
    {
        private readonly SolarDigestApiProps _apiProps;
        private readonly Iam _iam;
        private readonly DynamoDbTables _tables;
        private readonly IBucket _codeBucket;

        internal IFunction AddSiteFunction { get; private set; }
        internal IFunction UpdateSiteFunction { get; private set; }
        internal IFunction GetSiteFunction { get; private set; }
        internal IFunction HydrateAllSitesPowerFunction { get; private set; }
        internal IFunction HydrateSitePowerFunction { get; private set; }
        internal IFunction EmailExceptionFunction { get; private set; }
        internal IFunction AggregateAllSitesPowerFunction { get; private set; }
        internal IFunction AggregateSitePowerFunction { get; private set; }

        public Functions(Construct scope, SolarDigestApiProps apiProps, Iam iam, DynamoDbTables tables)
            : base(scope, "Functions")
        {
            _apiProps = apiProps.WhenNotNull(nameof(apiProps));
            _iam = iam.WhenNotNull(nameof(iam));
            _tables = tables.WhenNotNull(nameof(tables));

            _codeBucket = AwsBucket.FromBucketName(this, "CodeBucket", Constants.S3LambdaCodeBucketName);

            CreateGetSiteFunction();
            CreateAddSiteFunction();
            CreateUpdateSiteFunction();
            CreateEmailExceptionFunction();
            CreateHydrateAllSitesPowerFunction();
            CreateHydrateSitePowerFunction();
            CreateAggregateAllSitesPowerFunction();
            CreateAggregateSitePowerFunction();
        }

        private IFunction CreateFunction(string appName, string name, string description, double? memorySize = default)
            //IDictionary<string, string> variables = null)
        {
            //variables ??= new Dictionary<string, string>();

            var props = new FunctionProps
            {
                FunctionName = $"{appName}_{name}",
                Description = description,
                Handler = $"SolarDigest.Api::SolarDigest.Api.Functions.{name}::InvokeAsync",
                Runtime = Runtime.DOTNET_CORE_3_1,
                MemorySize = memorySize,
                Timeout = Duration.Minutes(5),
                Code = new S3Code(_codeBucket, Constants.S3CodeBucketKeyName),
                LogRetention = RetentionDays.ONE_WEEK
                //Environment = variables
            };

            var function = new Function(this, $"{name}Function", props);

            return function;
        }

        private void CreateGetSiteFunction()
        {
            GetSiteFunction =
                CreateFunction(_apiProps.AppName, Constants.Function.GetSite, "Get site details")
                    .AddPolicyStatements(_iam.GetDynamoDescribeTablePolicy(_tables.SiteTable.TableName))
                    .GrantReadTableData(_tables.SiteTable)
                    .GrantWriteTableData(_tables.ExceptionTable);
        }

        private void CreateAddSiteFunction()
        {
            AddSiteFunction =
                CreateFunction(_apiProps.AppName, Constants.Function.AddSite, "Add site details")
                    .AddPolicyStatements(_iam.GetDynamoDescribeTablePolicy(_tables.SiteTable.TableName))
                    .GrantWriteTableData(_tables.ExceptionTable)
                    .GrantReadWriteTableData(_tables.SiteTable);
        }

        private void CreateUpdateSiteFunction()
        {
            UpdateSiteFunction =
                CreateFunction(_apiProps.AppName, Constants.Function.UpdateSite, "Update site details")
                    .AddPolicyStatements(_iam.GetDynamoDescribeTablePolicy(_tables.SiteTable.TableName))
                    .GrantWriteTableData(_tables.ExceptionTable)
                    .GrantReadWriteTableData(_tables.SiteTable);
        }

        private void CreateEmailExceptionFunction()
        {
            // Note: the EmailException function would never write to ExceptionTable as it would potentially cause an endless loop of processing

            // exceptions are forwarded via a DynamoDb stream from the Exception table to the EmailException function

            EmailExceptionFunction =
                CreateFunction(_apiProps.AppName, Constants.Function.EmailException, "Sends unexpected exception reports via email")
                    .AddPolicyStatements(_iam.SendEmailPolicyStatement)
                    .AddEventSource(_tables.ExceptionTable)
                    .GrantStreamReadData(_tables.ExceptionTable);
        }

        private void CreateHydrateAllSitesPowerFunction()
        {
            HydrateAllSitesPowerFunction =
                CreateFunction(_apiProps.AppName, Constants.Function.HydrateAllSitesPower, "Hydrate power data for all sites")
                    .AddPolicyStatements(_iam.GetDynamoDescribeTablePolicy(_tables.SiteTable.TableName))
                    .AddPolicyStatements(_iam.PutDefaultEventBridgeEventsPolicyStatement)
                    .GrantReadTableData(_tables.SiteTable)
                    .GrantWriteTableData(_tables.ExceptionTable);
        }

        private void CreateHydrateSitePowerFunction()
        {
            HydrateSitePowerFunction =
                CreateFunction(_apiProps.AppName, Constants.Function.HydrateSitePower, "Hydrate power data for a specified site", 192)
                    .AddPolicyStatements(_iam.GetParameterPolicyStatement)
                    .AddPolicyStatements(_iam.PutDefaultEventBridgeEventsPolicyStatement)
                    .AddPolicyStatements(_iam.GetDynamoDescribeTablePolicy(_tables.SiteTable.TableName))
                    .AddPolicyStatements(_iam.GetDynamoBatchWriteTablePolicy(_tables.PowerTable.TableName))
                    .GrantWriteTableData(_tables.ExceptionTable, _tables.PowerUpdateHistoryTable)
                    .GrantReadWriteTableData(_tables.SiteTable);
        }

        private void CreateAggregateAllSitesPowerFunction()
        {
            AggregateAllSitesPowerFunction =
                CreateFunction(_apiProps.AppName, Constants.Function.AggregateAllSitesPower, "Aggregate power data for all sites")
                    .AddPolicyStatements(_iam.GetDynamoDescribeTablePolicy(_tables.SiteTable.TableName))
                    .AddPolicyStatements(_iam.PutDefaultEventBridgeEventsPolicyStatement)
                    .GrantReadTableData(_tables.SiteTable)
                    .GrantWriteTableData(_tables.ExceptionTable);
        }

        private void CreateAggregateSitePowerFunction()
        {
            AggregateSitePowerFunction =
                CreateFunction(_apiProps.AppName, Constants.Function.AggregateSitePower, "Aggregate power data for a specified site", 192)
                    //.AddPolicyStatements(_iam.GetParameterPolicyStatement)
                    .AddPolicyStatements(_iam.GetDynamoDescribeTablePolicy(_tables.SiteTable.TableName))
                    //.AddPolicyStatements(_iam.GetDynamoBatchWriteTablePolicy(_tables.PowerTable.TableName))
                    //.GrantWriteTableData(_tables.ExceptionTable, _tables.PowerUpdateHistoryTable)
                    .GrantReadWriteTableData(_tables.SiteTable);
        }
    }
}