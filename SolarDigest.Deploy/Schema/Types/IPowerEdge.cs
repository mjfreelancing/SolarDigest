using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    [SchemaType(GraphqlSchemaType.Type, "PowerEdge")]
    internal interface IPowerEdge
    {
        [SchemaTypeRequired]
        public ITimeWatts Node { get; }

        [SchemaTypeRequired]
        public string Cursor { get; }
    }
}