namespace SolarDigest.Models.UploadMultiPart
{
    public sealed class UploadMultiPartETag
    {
        public int PartNumber { get; set; }
        public string ETag { get; set; }
    }
}