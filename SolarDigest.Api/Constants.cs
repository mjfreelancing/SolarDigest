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
        }
    }
}