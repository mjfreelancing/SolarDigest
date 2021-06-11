using SolarDigest.Models;

namespace SolarDigest.Api.Functions.Payloads
{
    // associated with a lambda resolver request
    public sealed class UpdateSitePayload
    {
        public string Id { get; set; }

        public SiteDetails Site { get; set; }
        public SiteTimestamps Timestamps { get; set; }
    }
}