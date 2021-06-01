using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    [SchemaType(GraphqlSchemaType.Type, "Power")]
    internal interface IPower
    {
        [SchemaTypeRequired]
        public ITimeWatts[] Watts { get; }
    }
}