using System.Collections.Generic;

namespace SolarDigest.Models.UploadMultiPart
{
    public sealed class UploadMultiParts
    {
        public string UploadId { get; set; }
        public IEnumerable<UploadMultiPart> Parts { get; set; }
    }
}