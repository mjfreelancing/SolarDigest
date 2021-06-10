namespace SolarDigest.Api.Models
{
    public sealed class EmailContext
    {
        public string SourceEmail { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string PlainMessage { get; set; }
        public string HtmlMessage { get; set; }
    }
}