namespace SolarDigest.Api.Functions.Payloads
{
    // associated with a lambda resolver request
    public sealed class GetSitePowerSummaryPayload
    {
        public string SiteId { get; set; }
        public int? Limit { get; set; }
        public string StartCursor { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string MeterType { get; set; }           // MeterType
        public string SummaryType { get; set; }         // SummaryType
    }
}