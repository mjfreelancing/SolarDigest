namespace SolarDigest.Api.Events
{
    public sealed class AggregateSitePowerEvent
    {
        public string SiteId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}