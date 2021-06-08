namespace SolarDigest.Models
{
    public sealed class PowerUpdateHistory
    {
        public string TimestampUtc { get; set; }
        public string Site { get; set; }
        public string StartDateTime { get; set; }
        public string EndDateTime { get; set; }
        public string Status { get; set; }
    }
}