namespace SolarDigest.Api.Functions.Payloads
{
    public sealed class GetUploadMultiPartUrlsPayload : AppSyncPayloadBase
    {
        public string Filename { get; set; }
        public int PartCount { get; set; }
    }
}