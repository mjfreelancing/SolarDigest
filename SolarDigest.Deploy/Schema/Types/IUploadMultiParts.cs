using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    [SchemaType(GraphqlSchemaType.Type, "UploadMultiParts")]
    internal interface IUploadMultiParts
    {
        [SchemaTypeRequired]
        public string UploadId { get; }

        [SchemaTypeRequired]
        public IUploadMultiPart[] Parts { get; }
    }
}