using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Inputs
{
    [SchemaType("UploadUrlInput", GraphqlSchemaType.Input)]
    internal interface IUploadUrlInput
    {
        [SchemaTypeRequired]
        public string Filename { get; set; }
    }
}