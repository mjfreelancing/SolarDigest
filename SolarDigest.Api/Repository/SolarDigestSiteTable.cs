using Amazon.DynamoDBv2.DocumentModel;
using AutoMapper;
using SolarDigest.Api.Data;
using SolarDigest.Api.Logging;
using SolarDigest.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestSiteTable : SolarDigestTableBase, ISolarDigestSiteTable
    {
        public override string TableName => Constants.Table.Site;

        public SolarDigestSiteTable(IMapper mapper, IFunctionLogger logger)
            : base(mapper, logger)
        {
        }

        public async Task<Site> GetSiteAsync(string siteId, CancellationToken cancellationToken)
        {
            var siteDetails = await TableImpl.GetItemAsync<SiteEntity>(siteId, cancellationToken);
            return MapToSite(siteDetails);
        }

        public async Task<Site> AddSiteAsync(string siteId, SiteDetails siteDetails, CancellationToken cancellationToken)
        {
            var entity = MapToSiteEntity(siteDetails);
            entity.Id = siteId;
            
            await TableImpl.AddItemAsync(entity, cancellationToken).ConfigureAwait(false);

            var site = Mapper.Map<Site>(siteDetails);
            site.Id = siteId;

            return site;
        }

        public async Task<Site> UpsertSiteAsync(Site site, CancellationToken cancellationToken = default)
        {
            var entity = Mapper.Map<SiteEntity>(site);

            await TableImpl.AddItemAsync(entity, cancellationToken).ConfigureAwait(false);

            return await GetSiteAsync(site.Id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Site> UpsertSiteAsync(string siteId, SiteDetails siteDetails, SiteTimestamps siteTimestamps, CancellationToken cancellationToken = default)
        {
            var entity = MapToSiteEntity(siteDetails);
            entity.Id = siteId;
            entity.LastAggregationDate = siteTimestamps?.LastAggregationDate;
            entity.LastSummaryDate = siteTimestamps?.LastSummaryDate;
            entity.LastRefreshDateTime = siteTimestamps?.LastRefreshDateTime;

            await TableImpl.PutItemAsync(entity, cancellationToken).ConfigureAwait(false);

            return await GetSiteAsync(siteId, cancellationToken).ConfigureAwait(false);
        }

        public async IAsyncEnumerable<Site> GetAllSitesAsync(IEnumerable<string> properties, Action<ScanFilter> filterAction, 
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var sites = TableImpl!.ScanAsync<SiteEntity>(properties, null, cancellationToken);

            await foreach (var siteDetails in sites.WithCancellation(cancellationToken))
            {
                yield return MapToSite(siteDetails);
            }
        }

        private Site MapToSite(SiteEntity siteEntity)
        {
            return Mapper.Map<Site>(siteEntity);
        }

        private SiteEntity MapToSiteEntity(SiteDetails siteDetails)
        {
            return Mapper.Map<SiteEntity>(siteDetails);
        }
    }
}