using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    [SchemaType(GraphqlSchemaType.Type, "Site")]
    internal interface ISite
    {
        [SchemaTypeRequired]
        public string Id { get; }

        [SchemaTypeRequired]
        public string StartDate { get; }

        //[GraphqlTypeRequired]
        //public string ApiKey { get; }

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
        IPower Power([SchemaTypeRequired] MeterType meterType, [SchemaTypeRequired] SummaryType summaryType);
    }
}