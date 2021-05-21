using AllOverIt.Helpers;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Processors
{
    internal sealed class PowerYearlyProcessor : IPowerYearlyProcessor
    {
        private readonly ISolarDigestPowerTable _powerTable;
        private readonly ISolarDigestPowerYearlyTable _powerYearlyTable;
        private readonly IFunctionLogger _logger;

        public PowerYearlyProcessor(ISolarDigestPowerTable powerTable, ISolarDigestPowerYearlyTable powerYearlyTable, IFunctionLogger logger)
        {
            _powerTable = powerTable.WhenNotNull(nameof(powerTable));
            _powerYearlyTable = powerYearlyTable.WhenNotNull(nameof(powerYearlyTable));
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public Task ProcessAsync(Site site, DateTime startDate, DateTime endDate)
        {
            _logger.LogDebug($"Processing yearly aggregation for site {site.Id} between {startDate.GetSolarDateString()} and {endDate.GetSolarDateString()}");

            return Task.CompletedTask;
        }
    }
}