using SolarDigest.Api.Models.SolarEdge;
using System;

namespace SolarDigest.Api.Data
{
    public sealed class MeterPowerEntity : EntityCompositeBase
    {
        public string Site { get; set; }
        public string Date { get; set; }
        public string YearMonth { get; set; }
        public string Time { get; set; }
        public string MeterType { get; set; }
        public double Watts { get; set; }
        public double WattHour { get; set; }

        public MeterPowerEntity()
        {
        }

        public MeterPowerEntity(string site, DateTime timestamp, MeterType meterType, double watts, double wattHour)
        {
            Site = site;
            Date = $"{timestamp:yyyyMMdd}";
            YearMonth = $"{timestamp:yyyyMM}";
            Time = $"{timestamp:HHmm}";
            MeterType = $"{meterType}";
            Watts = watts;
            WattHour = wattHour;

            Id = $"{Site}_{Date}_{MeterType}";
            Sort = $"{Time}";
        }
    }
}