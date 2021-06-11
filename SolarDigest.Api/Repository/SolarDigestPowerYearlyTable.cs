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
    }
}