using SolarDigest.Models;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    public interface ISiteUpdater
    {
        Task UpdateLastAggregationDateAsync(Site site, DateTime date);
        Task UpdateLastSummaryDateAsync(Site site, DateTime date);
        Task UpdateLastRefreshDateTimeAsync(Site site, DateTime dateTime);
    }
}