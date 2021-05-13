namespace SolarDigest.Models
{
    public class SiteDetails
    {
        public string StartDate { get; set; }
        public string ApiKey { get; set; }                  // SolarEdge API Key
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string TimeZoneId { get; set; }
    }
}