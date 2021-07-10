using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Aws.Cdk.AppSync.Schema.Types;
using SolarDigest.Deploy.Schema.Inputs;

namespace SolarDigest.Deploy.Schema.Types
{
    [SchemaType("Site", GraphqlSchemaType.Type)]
    internal interface ISite
    {
        [SchemaTypeRequired]
        public string Id { get; }

        [SchemaTypeRequired]
        public AwsTypeDate StartDate { get; }

        [SchemaTypeRequired]
        public string ContactName { get; }

        [SchemaTypeRequired]
        public string ContactEmail { get; }

        [SchemaTypeRequired]
        public string TimeZoneId { get; }

        public string LastAggregationDate { get; }

        public string LastSummaryDate { get; }

        public string LastRefreshDateTime { get; }

        [SchemaTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.GetSitePowerSummary)]
        IPowerConnection Power(int limit, string startCursor, [SchemaTypeRequired] IPowerFilterInput filter);
    }
}