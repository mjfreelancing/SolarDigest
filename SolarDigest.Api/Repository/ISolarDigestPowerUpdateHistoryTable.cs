using SolarDigest.Api.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    public interface ISolarDigestPowerUpdateHistoryTable
    {
        Task UpsertPowerStatusHistoryAsync(string siteId, DateTime startDateTime, DateTime endDateTime, PowerUpdateStatus status,
            CancellationToken cancellationToken = default);
    }
}