using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Aws.Cdk.AppSync.Schema.Types;
using SolarDigest.Deploy.Schema.Types;

namespace SolarDigest.Deploy.Schema.Inputs
{
    [SchemaType("PowerFilterInput", GraphqlSchemaType.Input)]
    internal interface IPowerFilterInput
    {
        [SchemaTypeRequired]
        public AwsTypeDate StartDate { get; }

        [SchemaTypeRequired]
        public AwsTypeDate EndDate { get; }

        [SchemaTypeRequired]
        public MeterType MeterType { get; }

        [SchemaTypeRequired]
        public SummaryType SummaryType { get; }
    }
}