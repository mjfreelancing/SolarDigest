using SolarDigest.Models;

namespace SolarDigest.Api.Payloads.GraphQL
{
    public sealed class AddSitePayload
    {
        public string Id { get; set; }

        public SiteDetails Site { get; set; }       // only contains core details, no refresh details
    }
}