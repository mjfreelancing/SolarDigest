namespace SolarDigest.Api.Functions.Payloads
{
    public sealed class GetUploadUrlPayload : AppSyncPayloadBase
    {
        public string Filename { get; set; }
    }
}