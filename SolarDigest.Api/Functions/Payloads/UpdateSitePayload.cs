using SolarDigest.Models;

namespace SolarDigest.Api.Functions.Payloads
{
    public sealed class UpdateSitePayload : AppSyncPayloadBase
    {
        public string Id { get; set; }

        public SiteDetails Site { get; set; }
        public SiteTimestamps Timestamps { get; set; }
    }
}