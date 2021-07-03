using SolarDigest.Models;
using System.Collections.Generic;
using System.Linq;

namespace SolarDigest.Graphql.Responses
{
    public sealed class GetSitePowerResponse
    {
        public IEnumerable<PowerEdge> Edges { get; set; } = Enumerable.Empty<PowerEdge>();
        public int TotalCount { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}