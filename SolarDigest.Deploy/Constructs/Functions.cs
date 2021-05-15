using AllOverIt.Helpers;
using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
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

        public Functions(Construct scope, SolarDigestApiProps apiProps, Iam iam, DynamoDbTables tables)
            : base(scope, "Functions")
        {
            _apiProps = apiProps.WhenNotNull(nameof(apiProps));
            _iam = iam.WhenNotNull(nameof(iam));
            _tables = tables.WhenNotNull(nameof(tables));

            _codeBucket = AwsBucket.FromBucketName(this, "CodeBucket", Constants.S3LambdaCodeBucketName);

            CreateAddSiteFunction();
            CreateUpdateSiteFunction();
            CreateGetSiteFunction();
            CreateHydrateAllSitesPowerFunction();
            CreateHydrateSitePowerFunction();
            CreateEmailExceptionFunction();
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
                Code = new S3Code(_codeBucket, Constants.S3CodeBucketKeyName)
                //Environment = variables
            };

            var function = new Function(this, $"{name}Function", props);

            return function;
        }

        private void CreateAddSiteFunction()
        {
            AddSiteFunction = CreateFunction(_apiProps.AppName, Constants.Function.AddSite, "Add site details");

            AddSiteFunction.AddPolicyStatements(_iam.GetDynamoDescribeTablePolicy(_tables.SiteTable.TableName));

            _tables.ExceptionTable.GrantWriteData(AddSiteFunction);

            _tables.SiteTable.GrantReadWriteData(AddSiteFunction);
        }

        private void CreateUpdateSiteFunction()
        {
            UpdateSiteFunction = CreateFunction(_apiProps.AppName, Constants.Function.UpdateSite, "Update site details");

            UpdateSiteFunction.AddPolicyStatements(_iam.GetDynamoDescribeTablePolicy(_tables.SiteTable.TableName));

            _tables.ExceptionTable.GrantWriteData(UpdateSiteFunction);

            _tables.SiteTable.GrantReadWriteData(UpdateSiteFunction);
        }

        private void CreateGetSiteFunction()
        {
            GetSiteFunction = CreateFunction(_apiProps.AppName, Constants.Function.GetSite, "Get site details");

            GetSiteFunction.AddPolicyStatements(_iam.GetDynamoDescribeTablePolicy(_tables.SiteTable.TableName));

            _tables.ExceptionTable.GrantWriteData(GetSiteFunction);
            _tables.SiteTable.GrantReadData(GetSiteFunction);
        }

        private void CreateHydrateAllSitesPowerFunction()
        {
            HydrateAllSitesPowerFunction = CreateFunction(_apiProps.AppName, Constants.Function.HydrateAllSitesPower, "Hydrate power data for all sites");

            HydrateAllSitesPowerFunction.AddPolicyStatements(_iam.PutDefaultEventBridgeEventsPolicyStatement);

            _tables.ExceptionTable.GrantWriteData(HydrateAllSitesPowerFunction);
            _tables.SiteTable.GrantReadData(HydrateAllSitesPowerFunction);
        }

        private void CreateHydrateSitePowerFunction()
        {
            HydrateSitePowerFunction = CreateFunction(_apiProps.AppName, Constants.Function.HydrateSitePower, "Hydrate power data for a specified site", 192);

            HydrateSitePowerFunction.AddPolicyStatements(_iam.GetParameterPolicyStatement);
            HydrateSitePowerFunction.AddPolicyStatements(_iam.PutDefaultEventBridgeEventsPolicyStatement);
            HydrateSitePowerFunction.AddPolicyStatements(_iam.GetDynamoDescribeTablePolicy(_tables.SiteTable.TableName));
            HydrateSitePowerFunction.AddPolicyStatements(_iam.GetDynamoBatchWriteTablePolicy(_tables.PowerTable.TableName));

            _tables.ExceptionTable.GrantWriteData(HydrateSitePowerFunction);
            _tables.SiteTable.GrantReadData(HydrateSitePowerFunction);
        }

        private void CreateEmailExceptionFunction()
        {
            EmailExceptionFunction = CreateFunction(_apiProps.AppName, Constants.Function.EmailException, "Sends unexpected exception reports via email");

            EmailExceptionFunction.AddPolicyStatements(_iam.SendEmailPolicyStatement);

            // the EmailException function would never write to ExceptionTable as it would potentially cause an endless loop of processing
            //_tables.ExceptionTable.GrantWriteData(EmailExceptionFunction);

            // exceptions are forwarded via a DynamoDb stream from the Exception table to the EmailException function
            _tables.ExceptionTable.AddEventSource(EmailExceptionFunction);
            _tables.ExceptionTable.GrantStreamRead(EmailExceptionFunction);
        }
    }
}