using SolarDigest.Models;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Processors
{
    public interface IPowerProcessor
    {
        Task ProcessAsync(Site site, DateTime startDate, DateTime endDate);
    }
}