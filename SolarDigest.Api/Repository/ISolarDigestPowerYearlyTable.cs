using SolarDigest.Api.Models;
using SolarDigest.Api.Models.SolarEdge;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    public interface ISolarDigestPowerYearlyTable
    {
        Task AddMeterPowerAsync(IEnumerable<MeterPowerYear> powerData, CancellationToken cancellationToken = default);
        IAsyncEnumerable<MeterPowerYear> GetMeterDataAsync(string siteId, int year, MeterType meterType, CancellationToken cancellationToken = default);
    }
}