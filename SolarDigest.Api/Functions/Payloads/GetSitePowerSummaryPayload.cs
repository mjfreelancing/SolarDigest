namespace SolarDigest.Api.Functions.Payloads
{
    // associated with a lambda resolver request
    public sealed class GetSitePowerSummaryPayload
    {
        public string Id { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string MeterType { get; set; }           // MeterType
        public string SummaryType { get; set; }         // SummaryType
    }
}