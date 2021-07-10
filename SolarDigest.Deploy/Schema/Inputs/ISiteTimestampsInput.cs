using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Inputs
{
    [SchemaType("SiteTimestampsInput", GraphqlSchemaType.Input)]
    internal interface ISiteTimestampsInput
    {
        public string LastAggregationDate { get; }

        public string LastSummaryDate { get; }

        public string LastRefreshDateTime { get; }
    }
}