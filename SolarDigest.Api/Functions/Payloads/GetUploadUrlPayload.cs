namespace SolarDigest.Api.Functions.Payloads
{
    public sealed class GetUploadUrlPayload : AppSyncPayloadBase
    {
        public string Filename { get; set; }

        // only applicable if performing a multi-part upload
        public string UploadId { get; set; }
        public int? PartNumber { get; set; }
    }
}