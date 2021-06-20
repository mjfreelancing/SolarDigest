namespace SolarDigest.Cli.Commands.Upload
{
    internal class UploadPartResponse
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