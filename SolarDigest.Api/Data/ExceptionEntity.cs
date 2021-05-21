using SolarDigest.Shared.Extensions;
using System;

namespace SolarDigest.Api.Data
{
    public sealed class ExceptionEntity : EntityBase
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public long TimeToLive { get; set; }

        public ExceptionEntity()
        {
            Id = $"{Guid.NewGuid()}";
            TimeToLive = DateTime.UtcNow.AddDays(7).ToEpoch();
        }
    }
}