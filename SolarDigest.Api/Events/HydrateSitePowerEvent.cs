namespace SolarDigest.Api.Events
{
    public sealed class HydrateSitePowerEvent
    {
        public string SiteId { get; set; }

        // Optional. When not provided the refresh is performed based on the last refreshed timestamp and
        // what the current date/time is, less 1 hour (local time for that site).
        public string StartDateTime { get; set; }
        public string EndDateTime { get; set; }
    }
}