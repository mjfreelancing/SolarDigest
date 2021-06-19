using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Inputs
{
    [SchemaType(GraphqlSchemaType.Input, "UploadUrlInput")]
    internal interface IUploadUrlInput
    {
        [SchemaTypeRequired]
        public string Filename { get; set; }

        string UploadId { get; set; }
        int PartNumber { get; set; }
    }
}