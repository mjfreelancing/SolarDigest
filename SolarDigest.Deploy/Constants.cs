namespace SolarDigest.Deploy
{
    internal static class Constants
    {
        internal const string AppName = "SolarDigest";
        internal const int DataVersion = 1;
        internal const int ServiceVersion = 1;

        internal const string S3LambdaCodeBucketName = "solardigest-code";

        // should really be something like {ServiceName}-V{ApiVersion} - needs a file version too
        internal const string S3CodeBucketKeyName = "publish.zip";

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
        }
    }
}