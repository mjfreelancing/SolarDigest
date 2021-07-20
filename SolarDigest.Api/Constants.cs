namespace SolarDigest.Api
{
    internal static class Constants
    {
        internal static readonly Amazon.RegionEndpoint RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2;

        internal const string DateFormat = "yyyy-MM-dd";
        internal const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        internal const int DefaultPaginationLimit = 100;

        internal static class SolarEdge
        {
            public const string MonitoringUri = "https://monitoringapi.solaredge.com/";
        }

        internal static class Parameters
        {
            public const string SecretsRoot = "/Secrets";
        }

        internal static class Events
        {
            public const string Source = "SolarDigest.Api";
        }

        internal static class RefreshHour
        {
            public const int UpdateHistoryEmail = 1;        // 1am each day
            public const int SitePowerAggregation = 2;      // 2am each day
        }

        public static class AggregationOptions
        {
            public const string CultureName = "en-US";
        }
    }
}