using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    [GraphqlSchemaType(GraphqlSchemaType.Type, "Site")]
    internal interface ISite
    {
        [GraphqlTypeRequired]
        public string Id { get; }

        [GraphqlTypeRequired]
        public string StartDate { get; }

        //[GraphqlTypeRequired]
        //public string ApiKey { get; }

        [GraphqlTypeRequired]
        public string ContactName { get; }

        [GraphqlTypeRequired]
        public string ContactEmail { get; }

        [GraphqlTypeRequired]
        public string TimeZoneId { get; }

        public string LastAggregationDate { get; }

        public string LastSummaryDate { get; }

        public string LastRefreshDateTime { get; }

        [GraphqlTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.GetSitePowerSummary)]
        IPower Power([GraphqlTypeRequired] MeterType meterType, [GraphqlTypeRequired] SummaryType summaryType);
    }
}