using System.Collections.Generic;

namespace SolarDigest.Api.Models.SolarEdgeData
{
    public class MeterValues
    {
        public IEnumerable<Meter> Meters { get; set; }
    }
}