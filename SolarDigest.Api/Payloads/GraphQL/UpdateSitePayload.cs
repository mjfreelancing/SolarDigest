using SolarDigest.Models;

namespace SolarDigest.Api.Payloads.GraphQL
{
    public sealed class UpdateSitePayload
    {
        public string Id { get; set; }

        public SiteDetails Site { get; set; }
        public SiteTimestamps Timestamps { get; set; }
    }
}