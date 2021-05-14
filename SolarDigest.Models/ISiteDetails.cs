namespace SolarDigest.Models
{
    public interface ISiteDetails
    {
        public string StartDate { get; }
        public string ApiKey { get; }                  // SolarEdge API Key
        public string ContactName { get; }
        public string ContactEmail { get; }
        public string TimeZoneId { get; }
    }
}