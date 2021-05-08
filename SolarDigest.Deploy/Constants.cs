namespace SolarDigest.Deploy
{
    internal static class Constants
    {
        internal const int ApiVersion = 1;
        internal const string ServiceName = "SolarDigest";

        internal const string S3LambdaCodeBucketName = "solardigest-code";
        internal const string S3CodeBucketKeyName = "publish.zip";      // should really be something like {ServiceName}-V{ApiVersion}

        internal static class Function
        {
            internal const string GetSiteInfo = "GetSiteInfo";
            internal const string HydrateAllSitesPower = "HydrateAllSitesPower";
            internal const string HydrateSitePower = "HydrateSitePower";
            internal const string EmailException = "EmailException";
        }
    }
}