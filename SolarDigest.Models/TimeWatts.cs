namespace SolarDigest.Models
{
    public sealed class TimeWatts
    {
        public string Time { get; set; }
        public double Watts { get; set; }
        public double WattHour { get; set; }

        public static TimeWatts Create(string time, double watts, double wattHour)
        {
            return new TimeWatts
            {
                Time = time,
                Watts = watts,
                WattHour = wattHour
            };
        }
    }
}