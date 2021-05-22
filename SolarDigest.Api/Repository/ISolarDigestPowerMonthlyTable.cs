using SolarDigest.Api.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    public interface ISolarDigestPowerMonthlyTable
    {
        Task AddMeterPowerAsync(IEnumerable<MeterPowerMonth> powerData, CancellationToken cancellationToken = default);
    }
}