namespace SolarDigest.Deploy
{
    internal static class Constants
    {
        internal const string S3CodeBucketKeyName = "publish.zip";

        internal static class S3Buckets
        {
            internal const string LambdaSourceCodeBucketName = "code";
        }

        internal static class Function
        {
            internal const string GetSite = "GetSite";
            internal const string AddSite = "AddSite";
            internal const string UpdateSite = "UpdateSite";
            internal const string EmailException = "EmailException";
            internal const string HydrateAllSitesPower = "HydrateAllSitesPower";
            internal const string HydrateSitePower = "HydrateSitePower";
            internal const string AggregateAllSitesPower = "AggregateAllSitesPower";
            internal const string AggregateSitePower = "AggregateSitePower";
            internal const string GetSitePowerSummary = "GetSitePowerSummary";
            internal const string EmailAllSitesUpdateHistory = "EmailAllSitesUpdateHistory";
            internal const string GetUploadUrl = "GetUploadUrl";
            internal const string GetUploadMultiPartUrls = "GetUploadMultiPartUrls";
            internal const string GetUploadMultiPartAbort = "GetUploadMultiPartAbort";
            internal const string GetUploadMultiPartComplete = "GetUploadMultiPartComplete";
            internal const string GetDownloadUrl = "GetDownloadUrl";
        }
    }
}