using System.Collections.Generic;

namespace SolarDigest.Api.Functions.Responses
{
    public sealed class GetUploadMultiPartsResponse
    {
        public string UploadId { get; set; }
        public IEnumerable<GetUploadMultiPartResponse> Parts { get; set; }
    }
}