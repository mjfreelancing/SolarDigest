using System.Collections.Generic;

namespace SolarDigest.Models
{
    public sealed class SitePower
    {
        public IEnumerable<TimeWatts> TimeWatts { get; set; }
    }
}