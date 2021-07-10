namespace SolarDigest.Api.Functions.Payloads
{
    public sealed class GetSitePayload : AppSyncPayloadBase
    {
        public string Id { get; set; }
    }
}