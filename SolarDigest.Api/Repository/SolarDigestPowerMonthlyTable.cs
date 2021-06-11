using AllOverIt.Extensions;
using AutoMapper;
using SolarDigest.Api.Data;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestPowerMonthlyTable : SolarDigestTableBase, ISolarDigestPowerMonthlyTable
    {
        public override string TableName => Constants.Table.PowerMonthly;

        public SolarDigestPowerMonthlyTable(IMapper mapper, IFunctionLogger logger)
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
    }
}