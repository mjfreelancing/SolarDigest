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
        [LambdaDataSource(Constants.AppName, Constants.Function.GetUploadMultiPartUrls)]
        IUploadMultiParts UploadMultiPartUrls([SchemaTypeRequired] IUploadMultiPartInput input);

        [SchemaTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.GetUploadMultiPartAbort)]
        bool UploadMultiPartAbort([SchemaTypeRequired] IUploadMultiPartAbortInput input);

        [SchemaTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.GetUploadMultiPartComplete)]
        bool UploadMultiPartComplete([SchemaTypeRequired] IUploadMultiPartCompleteInput input);

        [SchemaTypeRequired]
        [LambdaDataSource(Constants.AppName, Constants.Function.GetDownloadUrl)]
        string DownloadUrl([SchemaTypeRequired] string filename);
    }
}