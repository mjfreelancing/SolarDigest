using AllOverIt.Extensions;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarDigest.Api.Functions.Responses
{
    public sealed class GetSitePowerResponse
    {
        public IEnumerable<PowerEdge> Edges { get; } = Enumerable.Empty<PowerEdge>();
        public IEnumerable<TimeWatts> Nodes { get; } = Enumerable.Empty<TimeWatts>();
        public int TotalCount { get; }
        public PageInfo PageInfo { get; }

        public GetSitePowerResponse(IEnumerable<TimeWatts> timeWatts, Func<TimeWatts, string> cursorResolver, Pagination pagination)
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
                    var previousPageStart = startIndex - pagination.Limit;

                    string previousCursor = default;

                    if (previousPageStart >= 0)
                    {
                        previousCursor = allData.ElementAt(previousPageStart).Time.ToBase64();
                    }
                    else
                    {
                        if (startIndex > 0)
                        {
                            previousCursor = allData.First().Time.ToBase64();
                        }
                    }

                    var nextPageStart = startIndex + pagination.Limit;

                    var nextCursor = nextPageStart < TotalCount
                        ? allData.ElementAt(nextPageStart).Time.ToBase64()
                        : null;

                    PageInfo = new(previousCursor, nextCursor);

                    Nodes = allData
                        .Skip(startIndex)
                        .Take(pagination.Limit)
                        .AsReadOnlyCollection();

                    Edges = Nodes
                        .Select(item => new PowerEdge
                        {
                            Node = item,
                            Cursor = cursorResolver.Invoke(item)
                        })
                        .AsReadOnlyCollection();
                }
            }
        }
    }
}