using SolarDigest.Api.Extensions;
using System;

namespace SolarDigest.Api.Data
{
    public abstract class EntityBase
    {
        public string Id { get; set; }
        public string TimestampUtc { get; set; }

        protected EntityBase()
        {
            TimestampUtc = DateTime.UtcNow.GetSolarDateTimeString();
        }
    }
}