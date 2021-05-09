﻿using AllOverIt.Helpers;
using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using SolarDigest.Deploy.Extensions;
using System.Collections.Generic;
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
        internal IFunction GetSiteFunction { get; private set; }
        internal IFunction HydrateAllSitesPowerFunction { get; private set; }
        internal IFunction HydrateSitePowerFunction { get; private set; }
        internal IFunction EmailExceptionFunction { get; private set; }

        public Functions(Construct scope, SolarDigestApiProps apiProps, Iam iam, DynamoDbTables tables)
            : base(scope, "Functions")
        {
            _apiProps = apiProps.WhenNotNull(nameof(apiProps));
            _iam = iam.WhenNotNull(nameof(iam));
            _tables = tables.WhenNotNull(nameof(tables));

            _codeBucket = AwsBucket.FromBucketName(this, "CodeBucket", Constants.S3LambdaCodeBucketName);

            CreateAddSiteFunction();
            CreateGetSiteFunction();
            CreateHydrateAllSitesPowerFunction();
            CreateHydrateSitePowerFunction();
            CreateEmailExceptionFunction();
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

            var function = new Function(this, $"{name}Function", props);

            foreach (var statement in statements)
            {
                function.AddToRolePolicy(statement);
            }

            return function;
        }

        private void CreateAddSiteFunction()
        {
            AddSiteFunction = CreateFunction(_apiProps.AppName, Constants.Function.AddSite, "Add site details", _codeBucket);

            _tables.ExceptionTable.GrantWriteData(AddSiteFunction);

            _tables.SiteTable.GrantWriteData(AddSiteFunction);
        }

        private void CreateGetSiteFunction()
        {
            GetSiteFunction = CreateFunction(_apiProps.AppName, Constants.Function.GetSite, "Get site details", _codeBucket,
                new[] { _iam.DynamoDescribeTablePolicy });

            _tables.ExceptionTable.GrantWriteData(GetSiteFunction);

            _tables.SiteTable.GrantReadData(GetSiteFunction);
        }

        private void CreateHydrateAllSitesPowerFunction()
        {
            HydrateAllSitesPowerFunction = CreateFunction(_apiProps.AppName, Constants.Function.HydrateAllSitesPower, "Hydrate power data for all sites", _codeBucket,
                new[] { _iam.PutDefaultEventBridgeEventsPolicyStatement });


            _tables.ExceptionTable.GrantWriteData(HydrateAllSitesPowerFunction);

            _tables.SiteTable.GrantReadData(HydrateAllSitesPowerFunction);
        }

        private void CreateHydrateSitePowerFunction()
        {
            HydrateSitePowerFunction = CreateFunction(_apiProps.AppName, Constants.Function.HydrateSitePower, "Hydrate power data for a specified site", _codeBucket);

            _tables.ExceptionTable.GrantWriteData(HydrateSitePowerFunction);

            _tables.SiteTable.GrantReadData(HydrateSitePowerFunction);
        }

        private void CreateEmailExceptionFunction()
        {
            EmailExceptionFunction = CreateFunction(_apiProps.AppName, Constants.Function.EmailException, "Sends unexpected exception reports via email", _codeBucket,
                new[] { _iam.SendEmailPolicyStatement });

            _tables.ExceptionTable.GrantWriteData(EmailExceptionFunction);

            // exceptions are forwarded via a DynamoDb stream from the Exception table to the EmailException function
            _tables.ExceptionTable.AddEventSource(EmailExceptionFunction);
            _tables.ExceptionTable.GrantStreamRead(EmailExceptionFunction);
        }
    }
}