using AllOverIt.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarDigest.Models
{
    public sealed class PowerConnection
    {
        public IEnumerable<PowerEdge> Edges { get; } = Enumerable.Empty<PowerEdge>();
        public IEnumerable<TimeWatts> Nodes { get; } = Enumerable.Empty<TimeWatts>();
        public int TotalCount { get; }
        public PageInfo PageInfo { get; } = new();

        public PowerConnection(IEnumerable<TimeWatts> timeWatts, Func<TimeWatts, string> cursorResolver, Pagination pagination)
        {
            var allData = timeWatts.ToList();

            TotalCount = allData.Count;

            if (TotalCount > 0)
            {
                var startIndex = 0;

                if (!pagination.StartCursor.IsNullOrEmpty())
                {
                    var startTime = pagination.StartCursor.FromBase64();

                    startIndex = allData.FindIndex(item => item.Time == startTime);
                }

                if (startIndex != -1)
                {
                    string previousCursor = null;
                    string nextCursor = null;

                    var previousPageStart = startIndex - pagination.Limit;

                    if (previousPageStart >= 0)
                    {
                        previousCursor = allData.ElementAt(previousPageStart).Time.ToBase64();
                    }

                    var nextPageStart = startIndex + pagination.Limit;

                    if (nextPageStart < TotalCount)
                    {
                        nextCursor = allData.ElementAt(nextPageStart).Time.ToBase64();
                    }

                    PageInfo = new PageInfo(previousCursor, nextCursor);

                    Nodes = allData
                        .Skip(startIndex)
                        .Take(pagination.Limit)
                        .AsReadOnlyCollection();

                    Edges = Nodes
                        .Select(item => new PowerEdge(item, cursorResolver.Invoke(item)))
                        .AsReadOnlyCollection();
                }
            }
        }
    }
}