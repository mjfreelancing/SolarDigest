using System.Collections.Generic;

namespace SolarDigest.Models.UploadMultiPart
{
    public sealed class UploadMultiParts
    {
        public string UploadId { get; set; }
        public IEnumerable<Models.UploadMultiPart.UploadMultiPart> Parts { get; set; }
    }
}