using System;

namespace SolarDigest.Api.Data
{
    public class ExceptionEntity
    {
        public string Id { get; set; }
        public string TimestampUtc { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }

        public ExceptionEntity()
        {
            Id = $"{Guid.NewGuid()}";
            TimestampUtc = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
        }
    }
}