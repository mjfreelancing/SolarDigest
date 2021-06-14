using AllOverIt.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarDigest.Models
{
    public sealed class PowerConnection
    {
        public IEnumerable<PowerEdge> Edges { get; }
        public int TotalCount { get; }
        public PageInfo PageInfo { get; }

        public PowerConnection(IEnumerable<TimeWatts> timeWatts, Func<TimeWatts, string> cursorResolver)
        {
            var timeWattsCollection = timeWatts
                .Select(item => new PowerEdge(item, cursorResolver.Invoke(item)))
                .AsReadOnlyCollection();

            Edges = timeWattsCollection;
            TotalCount = timeWattsCollection.Count;

            if (TotalCount > 0)
            {
                // todo: to be implemented
                PageInfo = new PageInfo(string.Empty, false);
            }
        }
    }
}