using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Aws.Cdk.AppSync.Schema.Types;

namespace SolarDigest.Deploy.Schema.Inputs
{
    [SchemaType("SiteInput", GraphqlSchemaType.Input)]
    internal interface ISiteInput
    {
        [SchemaTypeRequired]
        public AwsTypeDate StartDate { get; }

        [SchemaTypeRequired]
        public string ApiKey { get; }

        [SchemaTypeRequired]
        public string ContactName { get; }

        [SchemaTypeRequired]
        public string ContactEmail { get; }

        [SchemaTypeRequired]
        public string TimeZoneId { get; }
    }
}