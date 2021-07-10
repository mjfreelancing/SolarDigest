using SolarDigest.Api.Extensions;
using SolarDigest.Models;
using System;

namespace SolarDigest.Api.Models
{
    public sealed class MeterPowerYear
    {
        public string Site { get; set; }
        public int Year { get; set; }
        public string StartDate { get; set; }       // first date the data covers
        public string EndDate { get; set; }         // last date the data covers
        public string Time { get; set; }
        public int DayCount { get; set; }           // the first and last month may be partial months
        public string MeterType { get; set; }
        public double Watts { get; set; }
        public double WattHour { get; set; }

        public MeterPowerYear(string site, DateTime startDate, DateTime endDate, string time, MeterType meterType,
            double watts, double wattHour)
        {
            Site = site;
            Year = startDate.Year;
            StartDate = startDate.GetSolarDateString();
            EndDate = endDate.GetSolarDateString();
            Time = time;
            DayCount = (endDate - startDate).Days + 1;
            MeterType = $"{meterType}";
            Watts = Math.Round(watts, 6, MidpointRounding.AwayFromZero);
            WattHour = Math.Round(wattHour, 6, MidpointRounding.AwayFromZero);
        }
    }
}