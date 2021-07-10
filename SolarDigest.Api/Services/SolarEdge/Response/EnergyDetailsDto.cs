using System.Collections.Generic;

namespace SolarDigest.Api.Services.SolarEdge.Response
{
    public class EnergyDetailsDto
    {
        public IEnumerable<MeterDto> Meters { get; set; }
    }
}