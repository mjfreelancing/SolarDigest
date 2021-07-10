namespace SolarDigest.Api.Summarizers
{
    public sealed class PeriodWattData
    {
        public int DayCount { get; set; }
        public double Watts { get; set; }
        public double WattHour { get; set; }

        public PeriodWattData(int dayCount, double watts, double wattHour)
        {
            DayCount = dayCount;
            Watts = watts;
            WattHour = wattHour;
        }
    }
}