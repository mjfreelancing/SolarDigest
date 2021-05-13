using SolarDigest.Models;

namespace SolarDigest.Api.Payloads.GraphQL
{
    public sealed class UpdateSitePayload
    {
        public string Id { get; set; }

        public SiteExtendedDetails Site { get; set; }       // only contains core details, no refresh details
    }
}