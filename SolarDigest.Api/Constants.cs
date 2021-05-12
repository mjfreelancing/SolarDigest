namespace SolarDigest.Api
{
    internal static class Constants
    {
        internal static class SolarEdge
        {
            public const string MonitoringUri = "https://monitoringapi.solaredge.com/";
        }

        internal static class Parameters
        {
            public const string SolarEdgeApiKey = "/SolarDigest/SolarEdgeApiKey";
        }

        internal static class Events
        {
            public const string Source = "SolarDigest.Api";
        }

        internal static class Table
        {
            internal const string Site = "Site";
            internal const string Exception = "Exception";
            internal const string EnergyCosts = "EnergyCosts";
            internal const string Power = "Power";
            internal const string PowerMonthly = "PowerMonthly";
            internal const string PowerYearly = "PowerYearly";
            internal const string PowerUpdateHistory = "PowerUpdateHistory";
        }
    }
}