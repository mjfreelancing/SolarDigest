using SolarDigest.Api.Models;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    public interface ISolarDigestPowerUpdateHistoryTable
    {
        Task UpsertPowerStatusHistoryAsync(string siteId, DateTime startDateTime, DateTime endDateTime, PowerUpdateStatus status,
            CancellationToken cancellationToken = default);

        IAsyncEnumerable<PowerUpdateHistory> GetPowerUpdatesAsyncEnumerable(string siteId, DateTime startDate, DateTime endDate,
            CancellationToken cancellationToken = default);
    }
}