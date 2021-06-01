using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    [GraphqlSchemaType(GraphqlSchemaType.Type, "Power")]
    internal interface IPower
    {
        [GraphqlTypeRequired]
        public ITimeWatts[] Watts { get; }
    }
}