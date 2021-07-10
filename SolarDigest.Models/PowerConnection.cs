using System.Collections.Generic;
using System.Linq;

namespace SolarDigest.Models
{
    public sealed class PowerConnection
    {
        public IEnumerable<PowerEdge> Edges { get; set; } = Enumerable.Empty<PowerEdge>();
        public IEnumerable<TimeWatts> Nodes { get; set; } = Enumerable.Empty<TimeWatts>();
        public int TotalCount { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}