using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    public interface ISolarDigestPowerTable
    {
        Task AddMeterPowerAsync(SolarViewDay powerData, CancellationToken cancellationToken = default);
        IAsyncEnumerable<MeterPower> GetMeterPowerAsync(string siteId, DateTime date, MeterType meterType, CancellationToken cancellationToken = default);
    }
}