using System.Collections.Generic;

namespace SolarDigest.Api.Models.SolarEdgeData
{
    public class Meter
    {
        public string Type { get; set; }
        public IEnumerable<MeterValue> Values { get; set; }
    }
}