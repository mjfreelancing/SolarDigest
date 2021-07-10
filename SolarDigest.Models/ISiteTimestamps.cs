namespace SolarDigest.Models
{
    public interface ISiteTimestamps
    {
        public string LastAggregationDate { get; }
        public string LastSummaryDate { get; }
        public string LastRefreshDateTime { get; }
    }
}