using System;

namespace SolarDigest.Api.Data
{
    public sealed class ExceptionEntity : EntityBase
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }

        public ExceptionEntity()
        {
            Id = $"{Guid.NewGuid()}";
        }
    }
}