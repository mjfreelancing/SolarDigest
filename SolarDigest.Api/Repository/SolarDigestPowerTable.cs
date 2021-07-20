using AllOverIt.Extensions;
using AutoMapper;
using SolarDigest.Api.Data;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models.SolarEdge;
using SolarDigest.Models;
using SolarDigest.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestPowerTable : SolarDigestTableBase, ISolarDigestPowerTable
    {
        public override string TableName => $"{Helpers.GetAppVersionName()}_{Shared.Constants.Table.Power}";

        public SolarDigestPowerTable(IMapper mapper, ISolarDigestLogger logger)
            : base(mapper, logger)
        {
        }

        public Task AddMeterPowerAsync(SolarViewDay powerData, CancellationToken cancellationToken)
        {
            var entities = powerData.Meters
                .SelectMany(
                    meter => meter.Points,
                    (meter, point) => new MeterPowerEntity(powerData.SiteId, point.Timestamp, meter.MeterType, point.Watts, point.WattHour))
                .AsReadOnlyList();

            return TableImpl.PutBatchItemsAsync(entities, cancellationToken);
        }

        public async IAsyncEnumerable<MeterPower> GetMeterPowerAsync(string siteId, DateTime date, MeterType meterType,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var primaryKey = $"{siteId}_{date:yyyyMMdd}_{meterType}";
            var meterEntities = TableImpl.GetItemsAsync<MeterPowerEntity>(primaryKey, cancellationToken);

            await foreach (var entity in meterEntities.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                yield return Mapper.Map<MeterPower>(entity);
            }
        }
    }
}