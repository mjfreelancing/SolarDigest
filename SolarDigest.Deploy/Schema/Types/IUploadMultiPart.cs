using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    [SchemaType("UploadMultiPart", GraphqlSchemaType.Type)]
    internal interface IUploadMultiPart
    {
        [SchemaTypeRequired]
        public int PartNumber { get; }

        [SchemaTypeRequired]
        public string Url { get; }
    }
}