using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Inputs
{
    [SchemaType(GraphqlSchemaType.Input, "UploadMultiPartAbortInput")]
    internal interface IUploadMultiPartAbortInput
    {
        [SchemaTypeRequired]
        string Filename { get; set; }

        [SchemaTypeRequired]
        string UploadId { get; set; }
    }
}