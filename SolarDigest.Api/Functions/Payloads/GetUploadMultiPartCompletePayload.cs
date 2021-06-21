using SolarDigest.Models;
using System.Collections.Generic;

namespace SolarDigest.Api.Functions.Payloads
{
    public sealed class GetUploadMultiPartCompletePayload : AppSyncPayloadBase
    {
        public string Filename { get; set; }
        public string UploadId { get; set; }
        public IEnumerable<UploadMultiPartETag> ETags { get; set; }
    }
}