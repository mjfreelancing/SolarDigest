using AllOverIt.Extensions;
using System.Collections.Generic;

namespace SolarDigest.Models
{
    public sealed class SitePower
    {
        public IEnumerable<TimeWatts> TimeWatts { get; }

        public SitePower(IEnumerable<TimeWatts> timeWatts)
        {
            TimeWatts = timeWatts.AsReadOnlyCollection();
        }
    }
}