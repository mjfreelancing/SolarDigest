namespace SolarDigest.Deploy
{
    internal static class Constants
    {
        internal const int ApiVersion = 1;
        internal const string ServiceName = "SolarDigest";

        internal static class DataSource
        {
            internal const string GetSite = "GetSite";
            internal const string CreateSite = "CreateSite";
            internal const string HydrateAllSitesPower = "HydrateAllSitesPower";
            internal const string HydrateSitePower = "HydrateSitePower";
            internal const string EmailException = "EmailException";
        }
    }
}