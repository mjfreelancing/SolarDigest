namespace SolarDigest.Models
{
    public class SiteTimestamps : ISiteTimestamps
    {
        public string LastAggregationDate { get; set; }
        public string LastSummaryDate { get; set; }
        public string LastRefreshDateTime { get; set; }
    }
}