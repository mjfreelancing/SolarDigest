namespace SolarDigest.Api.Functions.Payloads
{
    public sealed class GetUploadMultiPartAbortPayload : AppSyncPayloadBase
    {
        public string Filename { get; set; }
        public string UploadId { get; set; }
    }
}