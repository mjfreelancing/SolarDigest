namespace SolarDigest.Api.Payloads.GraphQL
{
    // associated with a lambda resolver request
    public sealed class GetSitePayload
    {
        public string Id { get; set; }
        public string StartDate { get; set; }
        public string ApiKey { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string TimeZoneId { get; set; }
        public string LastAggregationDate { get; set; }
        public string LastSummaryDate { get; set; }
        public string LastRefreshDateTime { get; set; }
    }
}