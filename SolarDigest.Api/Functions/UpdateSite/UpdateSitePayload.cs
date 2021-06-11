using SolarDigest.Models;

namespace SolarDigest.Api.Functions.UpdateSite
{
    // associated with a lambda resolver request
    public sealed class UpdateSitePayload
    {
        public string Id { get; set; }

        public SiteDetails Site { get; set; }
        public SiteTimestamps Timestamps { get; set; }
    }
}