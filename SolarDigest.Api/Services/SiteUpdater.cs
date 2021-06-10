using AllOverIt.Extensions;
using AllOverIt.Helpers;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Repository;
using SolarDigest.Models;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    internal sealed class SiteUpdater : ISiteUpdater
    {
        private readonly ISolarDigestSiteTable _siteTable;
        private readonly IFunctionLogger _logger;

        public SiteUpdater(ISolarDigestSiteTable siteTable, IFunctionLogger logger)
        {
            _siteTable = siteTable.WhenNotNull(nameof(siteTable));
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public Task UpdateLastAggregationDateAsync(Site site, DateTime date)
        {
            if (!site.LastAggregationDate.IsNullOrEmpty() && site.LastAggregationDate.ParseSolarDate() > date)
            {
                _logger.LogDebug($"Site {site.Id} already has a newer 'LastAggregationDate' so not updating ({site.LastAggregationDate} " +
                                 $"compared to {date.GetSolarDateString()})");

                return Task.CompletedTask;
            }

            site.LastAggregationDate = date.GetSolarDateString();

            _logger.LogDebug($"Updating site {site.Id} last aggregation date as {site.LastAggregationDate} (local)");

            // todo: handle concurrency issues - reload the site table only if there is a conflict
            return _siteTable.UpsertSiteAsync(site);
        }

        public Task UpdateLastSummaryDateAsync(Site site, DateTime date)
        {
            if (!site.LastSummaryDate.IsNullOrEmpty() && site.LastSummaryDate.ParseSolarDate() > date)
            {
                _logger.LogDebug($"Site {site.Id} already has a newer 'LastLastSummaryDateDate' so not updating ({site.LastSummaryDate} " +
                                 $"compared to {date.GetSolarDateString()})");

                return Task.CompletedTask;
            }

            site.LastSummaryDate = date.GetSolarDateString();

            _logger.LogDebug($"Updating site {site.Id} last summary date as {site.LastSummaryDate} (local)");

            // todo: handle concurrency issues - reload the site table only if there is a conflict
            return _siteTable.UpsertSiteAsync(site);
        }

        public Task UpdateLastRefreshDateTimeAsync(Site site, DateTime dateTime)
        {
            if (!site.LastRefreshDateTime.IsNullOrEmpty() && site.LastRefreshDateTime.ParseSolarDateTime() > dateTime)
            {
                _logger.LogDebug($"Site {site.Id} already has a newer 'LastRefreshDateTime' so not updating ({site.LastRefreshDateTime} " +
                                 $"compared to {dateTime.GetSolarDateTimeString()})");

                return Task.CompletedTask;
            }

            site.LastRefreshDateTime = dateTime.GetSolarDateTimeString();

            _logger.LogDebug($"Updating site {site.Id} last refresh timestamp as {site.LastRefreshDateTime} (local)");

            // todo: handle concurrency issues - reload the site table only if there is a conflict
            return _siteTable.UpsertSiteAsync(site);
        }
    }
}