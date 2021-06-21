using System.Collections.Generic;

namespace SolarDigest.Models
{
    public sealed class UploadMultiParts
    {
        public string UploadId { get; set; }
        public IEnumerable<UploadMultiPart> Parts { get; set; }
    }
}