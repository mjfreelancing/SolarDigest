using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Inputs
{
    [SchemaType(GraphqlSchemaType.Input, "UploadMultiPartCompleteInput")]
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