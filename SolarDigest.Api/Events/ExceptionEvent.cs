namespace SolarDigest.Api.Events
{
    public sealed class ExceptionEvent
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}