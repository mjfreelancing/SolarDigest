using SolarDigest.Api.Extensions;

namespace SolarDigest.Api.Functions.Payloads
{
    public sealed class GetSitePowerSummaryPayload : AppSyncPayloadBase, IRequiresNormalisation
    {
        public string SiteId { get; set; }
        public int? Limit { get; set; }
        public string StartCursor { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string MeterType { get; set; }           // of type MeterType
        public string SummaryType { get; set; }         // of type SummaryType

        public void Normalise()
        {
            MeterType = MeterType.NormaliseEnumValue();
            SummaryType = SummaryType.NormaliseEnumValue();
        }
    }
}