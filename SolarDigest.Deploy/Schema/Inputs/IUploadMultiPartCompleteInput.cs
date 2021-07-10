using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Inputs
{
    [SchemaType("UploadMultiPartCompleteInput", GraphqlSchemaType.Input)]
    internal interface IUploadMultiPartCompleteInput
    {
        [SchemaTypeRequired]
        public string Filename { get; set; }

        [SchemaTypeRequired]
        string UploadId { get; set; }

        [SchemaTypeRequired]
        IUploadMultiPartETagInput[] ETags { get; }
    }
}