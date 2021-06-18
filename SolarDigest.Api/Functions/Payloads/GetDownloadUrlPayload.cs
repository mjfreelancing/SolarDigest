namespace SolarDigest.Api.Functions.Payloads
{
    public sealed class GetDownloadUrlPayload : AppSyncPayloadBase
    {
        public string Filename { get; set; }
    }
}