using Amazon.DynamoDBv2.DocumentModel;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    public interface ISolarDigestSiteTable
    {
        Task<Site> GetSiteAsync(string siteId, CancellationToken cancellationToken = default);
        Task<Site> AddSiteAsync(string siteId, SiteDetails siteDetails, CancellationToken cancellationToken = default);
        Task<Site> UpsertSiteAsync(Site site, CancellationToken cancellationToken = default);
        Task<Site> UpsertSiteAsync(string siteId, SiteDetails siteDetails, SiteTimestamps siteTimestamps, CancellationToken cancellationToken = default);
        IAsyncEnumerable<Site> GetAllSitesAsync(IEnumerable<string> properties = null, Action<ScanFilter> filterAction = null, CancellationToken cancellationToken = default);
    }
}