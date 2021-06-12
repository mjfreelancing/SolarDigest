using AllOverIt.Extensions;
using AutoMapper;
using SolarDigest.Api.Data;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models;
using SolarDigest.Api.Models.SolarEdge;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestPowerYearlyTable : SolarDigestTableBase, ISolarDigestPowerYearlyTable
    {
        public override string TableName => Constants.Table.PowerYearly;

        public SolarDigestPowerYearlyTable(IMapper mapper, IFunctionLogger logger)
            : base(mapper, logger)
        {
        }

        public Task AddMeterPowerAsync(IEnumerable<MeterPowerYear> powerData, CancellationToken cancellationToken)
        {
            var entities = powerData
                .Select(Mapper.Map<MeterPowerYearEntity>)
                .AsReadOnlyCollection();

            return TableImpl.PutBatchItemsAsync(entities, cancellationToken);
        }

        public async IAsyncEnumerable<MeterPowerYear> GetMeterDataAsync(string siteId, int year, MeterType meterType,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var partitionKey = $"{siteId}_{year}_{meterType}";

            var entities = TableImpl.GetItemsAsync<MeterPowerYearEntity>(partitionKey, cancellationToken);

            await foreach (var entity in entities.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                yield return Mapper.Map<MeterPowerYear>(entity);
            }
        }
    }
}