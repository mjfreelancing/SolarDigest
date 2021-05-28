namespace SolarDigest.Api.Payloads.GraphQL
{
    public sealed class GetSitePowerSummaryPayload
    {
        public string Id { get; set; }
        public string MeterType { get; set; }           // MeterType
        public string SummaryType { get; set; }         // SummaryType
    }
}