using AutoMapper;
using SolarDigest.Api.Data;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models;
using SolarDigest.Models;
using SolarDigest.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestPowerUpdateHistoryTable : SolarDigestTableBase, ISolarDigestPowerUpdateHistoryTable
    {
        public override string TableName => $"{Helpers.GetAppVersionName()}_{Shared.Constants.Table.PowerUpdateHistory}";

        public SolarDigestPowerUpdateHistoryTable(IMapper mapper, ISolarDigestLogger logger)
            : base(mapper, logger)
        {
        }

        public Task UpsertPowerStatusHistoryAsync(string siteId, DateTime startDateTime, DateTime endDateTime, PowerUpdateStatus status,
            CancellationToken cancellationToken = default)
        {
            var entity = new PowerUpdateHistoryEntity(siteId, startDateTime, endDateTime, status);
            return TableImpl.PutItemAsync(entity, cancellationToken);
        }

        public async IAsyncEnumerable<PowerUpdateHistory> GetPowerUpdatesAsync(string siteId, DateTime startDate, DateTime endDate,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var primaryKey = $"{siteId}_{date:yyyy-MM-dd}";

                Logger.LogDebug($"Loading entities for PK '{primaryKey}'");

                var historyEntities = TableImpl.GetItemsAsync<PowerUpdateHistoryEntity>(primaryKey, cancellationToken);

                await foreach (var entity in historyEntities.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }

                    yield return Mapper.Map<PowerUpdateHistory>(entity);
                }
            }
        }
    }
}