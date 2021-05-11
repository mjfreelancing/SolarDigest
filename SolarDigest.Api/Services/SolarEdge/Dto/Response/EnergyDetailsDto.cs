using System.Collections.Generic;

namespace SolarDigest.Api.Services.SolarEdge.Dto.Response
{
    public class EnergyDetailsDto
    {
        public IEnumerable<MeterDto> Meters { get; set; }
    }
}