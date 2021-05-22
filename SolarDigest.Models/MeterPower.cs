namespace SolarDigest.Models
{
    public sealed class MeterPower
    {
        public string Site { get; set; }
        public string Date { get; set; }
        public string YearMonth { get; set; }
        public string Time { get; set; }
        public string MeterType { get; set; }
        public double Watts { get; set; }
        public double WattHour { get; set; }
    }
}