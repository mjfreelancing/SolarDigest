using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    [SchemaType(GraphqlSchemaType.Type, "PowerConnection")]
    internal interface IPowerConnection
    {
        [SchemaTypeRequired]
        public IPowerEdge[] Edges { get; }

        [SchemaTypeRequired]
        public int TotalCount { get; }

        // not required since TotalCount can be zero
        public IPageInfo PageInfo { get; }
    }
}