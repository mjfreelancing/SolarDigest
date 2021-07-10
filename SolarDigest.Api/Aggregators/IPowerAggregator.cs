using SolarDigest.Models;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Aggregators
{
    public interface IPowerAggregator
    {
        Task ProcessAsync(Site site, DateTime startDate, DateTime endDate);
    }
}