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

        public async Task UpdateLastAggregationDateAsync(string siteId, DateTime date)
        {
            var site = await _siteTable.GetSiteAsync(siteId);

            if (!site.LastAggregationDate.IsNullOrEmpty() && site.LastAggregationDate.ParseSolarDate() >= date)
            {
                _logger.LogDebug($"Site {site.Id} already has a {nameof(ISite.LastAggregationDate)} of {site.LastAggregationDate}. " +
                                 $"Not updating to {date.GetSolarDateString()})");

                return;
            }

            site.LastAggregationDate = date.GetSolarDateString();

            _logger.LogDebug($"Updating site {site.Id} last aggregation date as {site.LastAggregationDate} (local)");

            await _siteTable.UpsertSiteAsync(site).ConfigureAwait(false);
        }

        public async Task UpdateLastSummaryDateAsync(string siteId, DateTime date)
        {
            var site = await _siteTable.GetSiteAsync(siteId);

            if (!site.LastSummaryDate.IsNullOrEmpty() && site.LastSummaryDate.ParseSolarDate() >= date)
            {
                _logger.LogDebug($"Site {site.Id} already has a {nameof(ISite.LastSummaryDate)} of {site.LastSummaryDate}. " +
                                 $"Not updating to {date.GetSolarDateString()})");

                return;
            }

            site.LastSummaryDate = date.GetSolarDateString();

            _logger.LogDebug($"Updating site {site.Id} last summary date as {site.LastSummaryDate} (local)");

            await _siteTable.UpsertSiteAsync(site).ConfigureAwait(false);
        }

        public async Task UpdateLastRefreshDateTimeAsync(string siteId, DateTime dateTime)
        {
            var site = await _siteTable.GetSiteAsync(siteId);

            if (!site.LastRefreshDateTime.IsNullOrEmpty() && site.LastRefreshDateTime.ParseSolarDateTime() >= dateTime)
            {
                _logger.LogDebug($"Site {site.Id} already has a {nameof(ISite.LastRefreshDateTime)} of {site.LastRefreshDateTime}. " +
                                 $"Not updating to {dateTime.GetSolarDateTimeString()})");

                return;
            }

            site.LastRefreshDateTime = dateTime.GetSolarDateTimeString();

            _logger.LogDebug($"Updating site {site.Id} last refresh timestamp as {site.LastRefreshDateTime} (local)");

            await _siteTable.UpsertSiteAsync(site).ConfigureAwait(false);
        }
    }
}