using SolarDigest.Api.Models;
using SolarDigest.Api.Models.SolarEdge;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    public interface ISolarDigestPowerMonthlyTable
    {
        Task AddMeterPowerAsync(IEnumerable<MeterPowerMonth> powerData, CancellationToken cancellationToken = default);
        IAsyncEnumerable<MeterPowerMonth> GetMeterDataAsync(string siteId, int year, int month, MeterType meterType, CancellationToken cancellationToken = default);
    }
}