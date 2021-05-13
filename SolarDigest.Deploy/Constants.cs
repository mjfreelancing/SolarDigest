namespace SolarDigest.Deploy
{
    internal static class Constants
    {
        internal const int ApiVersion = 1;
        internal const string ServiceName = "SolarDigest";

        internal const string S3LambdaCodeBucketName = "solardigest-code";

        // should really be something like {ServiceName}-V{ApiVersion} - needs a file version too
        internal const string S3CodeBucketKeyName = "publish0001.zip";

        internal static class Function
        {
            internal const string GetSite = "GetSite";
            internal const string AddSite = "AddSite";
            internal const string HydrateAllSitesPower = "HydrateAllSitesPower";
            internal const string HydrateSitePower = "HydrateSitePower";
            internal const string EmailException = "EmailException";
        }
    }
}