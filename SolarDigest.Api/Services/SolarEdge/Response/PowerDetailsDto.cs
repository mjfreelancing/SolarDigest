using System.Collections.Generic;

namespace SolarDigest.Api.Services.SolarEdge.Response
{
    public class PowerDetailsDto
    {
        public IEnumerable<MeterDto> Meters { get; set; }
    }
}