using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    // Note: Does not implement conflict detection; last caller wins. Each method will, however, only update the date/time if it is newer.
    public interface ISiteUpdater
    {
        Task UpdateLastAggregationDateAsync(string siteId, DateTime date);
        Task UpdateLastSummaryDateAsync(string siteId, DateTime date);
        Task UpdateLastRefreshDateTimeAsync(string siteId, DateTime dateTime);
    }
}