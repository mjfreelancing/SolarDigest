using AllOverIt.Aws.Cdk.AppSync.Attributes;
using AllOverIt.Aws.Cdk.AppSync.Schema;
using SolarDigest.Deploy.Schema.Inputs;
using SolarDigest.Deploy.Schema.Types;

namespace SolarDigest.Deploy.Schema
{
    internal interface ISolarDigestQueryDefinition : IQueryDefinition
    {
        [SchemaTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.GetSite)]
        ISite Site([SchemaTypeRequired] string id);

        [SchemaTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.GetUploadUrl)]
        string UploadUrl([SchemaTypeRequired] IUploadUrlInput input);

        [SchemaTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.GetUploadParts)]
        IUploadMultiParts UploadMultiPartUrls([SchemaTypeRequired] IUploadMultiPartInput input);

        [SchemaTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.GetUploadPartsAbort)]
        string UploadMultiPartAbort([SchemaTypeRequired] IUploadMultiPartAbortInput input);

        [SchemaTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.GetUploadPartsComplete)]
        string UploadMultiPartComplete([SchemaTypeRequired] IUploadMultiPartCompleteInput input);

        [SchemaTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.GetDownloadUrl)]
        string DownloadUrl([SchemaTypeRequired] string filename);
    }
}