using System.Collections.Generic;

namespace SolarDigest.Api.Services.SolarEdge.Response
{
    public class MeterDto
    {
        public string Type { get; set; }
        public IEnumerable<MeterValueDto> Values { get; set; }
    }
}