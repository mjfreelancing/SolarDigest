using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    [SchemaType(GraphqlSchemaType.Type, "PageInfo")]
    internal interface IPageInfo
    {
        public string PreviousPageCursor { get; }
        public string NextPageCursor { get; }
    }
}