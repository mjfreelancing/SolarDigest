namespace SolarDigest.Models
{
    public sealed class UploadPartResponse
    {
        public int PartNumber { get; }
        public string ETag { get; }

        public UploadPartResponse(int partNumber, string eTag)
        {
            PartNumber = partNumber;
            ETag = eTag;
        }
    }
}