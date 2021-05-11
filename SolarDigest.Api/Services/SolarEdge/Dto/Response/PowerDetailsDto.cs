using System.Collections.Generic;

namespace SolarDigest.Api.Services.SolarEdge.Dto.Response
{
    public class PowerDetailsDto
    {
        public IEnumerable<MeterDto> Meters { get; set; }
    }
}