using AllOverIt.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarDigest.Models
{
    public sealed class PowerConnection
    {
        public IEnumerable<PowerEdge> Edges { get; set; } = Enumerable.Empty<PowerEdge>();
        public IEnumerable<TimeWatts> Nodes { get; set; } = Enumerable.Empty<TimeWatts>();
        public int TotalCount { get; set; }
        public PageInfo PageInfo { get; set; } = new();

        public static PowerConnection Create(IEnumerable<TimeWatts> timeWatts, Func<TimeWatts, string> cursorResolver, Pagination pagination)
        {
            var allData = timeWatts.ToList();

            var powerConnection = new PowerConnection
            {
                TotalCount = allData.Count
            };

            if (powerConnection.TotalCount > 0)
            {
                var startIndex = 0;

                if (!pagination.StartCursor.IsNullOrEmpty())
                {
                    var startTime = pagination.StartCursor.FromBase64();
                    startIndex = allData.FindIndex(item => item.Time == startTime);
                }

                if (startIndex != -1)
                {
                    var previousPageStart = startIndex - pagination.Limit;

                    var previousCursor = previousPageStart >= 0
                        ? allData.ElementAt(previousPageStart).Time.ToBase64()
                        : allData.First().Time.ToBase64();

                    var nextPageStart = startIndex + pagination.Limit;

                    var nextCursor = nextPageStart < powerConnection.TotalCount
                        ? allData.ElementAt(nextPageStart).Time.ToBase64()
                        : null;

                    powerConnection.PageInfo = new(previousCursor, nextCursor);

                    powerConnection.Nodes = allData
                        .Skip(startIndex)
                        .Take(pagination.Limit)
                        .AsReadOnlyCollection();

                    powerConnection.Edges = powerConnection.Nodes
                        .Select(item => PowerEdge.Create(item, cursorResolver.Invoke(item)))
                        .AsReadOnlyCollection();
                }
            }

            return powerConnection;
        }
    }
}