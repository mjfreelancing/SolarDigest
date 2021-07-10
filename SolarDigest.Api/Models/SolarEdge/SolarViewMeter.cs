using SolarDigest.Models;
using System.Collections.Generic;

namespace SolarDigest.Api.Models.SolarEdge
{
    public class SolarViewMeter
    {
        public MeterType MeterType { get; set; }
        public IEnumerable<SolarViewMeterPoint> Points { get; set; }
    }
}