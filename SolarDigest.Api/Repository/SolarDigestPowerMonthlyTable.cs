﻿using AllOverIt.Extensions;
using AutoMapper;
using SolarDigest.Api.Data;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models;
using SolarDigest.Models;
using SolarDigest.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestPowerMonthlyTable : SolarDigestTableBase, ISolarDigestPowerMonthlyTable
    {
        public override string TableName => $"{Helpers.GetAppVersionName()}_{Shared.Constants.Table.PowerMonthly}";

        public SolarDigestPowerMonthlyTable(IMapper mapper, ISolarDigestLogger logger)
            : base(mapper, logger)
        {
        }

        public Task AddMeterPowerAsync(IEnumerable<MeterPowerMonth> powerData, CancellationToken cancellationToken)
        {
            var entities = powerData
                .Select(Mapper.Map<MeterPowerMonthEntity>)
                .AsReadOnlyCollection();

            return TableImpl.PutBatchItemsAsync(entities, cancellationToken);
        }

        public async IAsyncEnumerable<MeterPowerMonth> GetMeterDataAsync(string siteId, int year, int month, MeterType meterType,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var partitionKey = $"{siteId}_{year}{month:D2}_{meterType}";

            var entities = TableImpl.GetItemsAsync<MeterPowerMonthEntity>(partitionKey, cancellationToken);

            await foreach (var entity in entities.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                yield return Mapper.Map<MeterPowerMonth>(entity);
            }
        }
    }
}