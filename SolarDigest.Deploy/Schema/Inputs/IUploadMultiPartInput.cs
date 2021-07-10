using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Inputs
{
    [SchemaType("UploadMultiPartInput", GraphqlSchemaType.Input)]
    internal interface IUploadMultiPartInput
    {
        [SchemaTypeRequired]
        public string Filename { get; set; }

        [SchemaTypeRequired]
        int PartCount { get; set; }
    }
}