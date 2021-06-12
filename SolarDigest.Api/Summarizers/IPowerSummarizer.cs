using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarDigest.Api.Summarizers
{
    public interface IPowerSummarizer
    {
        Task<IEnumerable<TimeWatts>> GetTimeWattsAsync(string siteId, MeterType meterType, DateTime startDate, DateTime endDate);
    }
}