namespace SolarDigest.Api.Functions.GetSitePowerSummary
{
    // associated with a lambda resolver request
    public sealed class GetSitePowerSummaryPayload
    {
        public string Id { get; set; }
        public string MeterType { get; set; }           // MeterType
        public string SummaryType { get; set; }         // SummaryType
    }
}