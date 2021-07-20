using SolarDigest.Models;

namespace SolarDigest.Api.Functions.Payloads
{
    public sealed class AddSitePayload : AppSyncPayloadBase
    {
        public string Id { get; set; }

        public SiteDetailsWithSecrets Site { get; set; }       // only contains core details, no refresh details
    }
}