using System;

namespace SolarDigest.Api.Models.SolarEdge
{
    public class SolarViewMeterPoint
    {
        public DateTime Timestamp { get; set; }
        public double Watts { get; set; }
        public double WattHour { get; set; }
    }
}