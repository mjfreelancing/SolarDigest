using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Inputs
{
    [SchemaType("UploadMultiPartAbortInput", GraphqlSchemaType.Input)]
    internal interface IUploadMultiPartAbortInput
    {
        [SchemaTypeRequired]
        string Filename { get; set; }

        [SchemaTypeRequired]
        string UploadId { get; set; }
    }
}