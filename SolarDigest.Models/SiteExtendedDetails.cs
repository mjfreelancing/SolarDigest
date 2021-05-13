namespace SolarDigest.Models
{
    public class SiteExtendedDetails : SiteDetails
    {
        public string LastAggregationDate { get; set; }
        public string LastSummaryDate { get; set; }
        public string LastRefreshDateTime { get; set; }
    }
}