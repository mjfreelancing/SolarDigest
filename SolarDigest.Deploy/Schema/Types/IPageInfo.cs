using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    [SchemaType(GraphqlSchemaType.Type, "PageInfo")]
    internal interface IPageInfo
    {
        [SchemaTypeRequired]
        public string StartCursor { get; }
        
        [SchemaTypeRequired]
        public bool HasNextPage { get; }
    }
}