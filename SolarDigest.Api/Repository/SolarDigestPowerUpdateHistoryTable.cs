using AutoMapper;
using SolarDigest.Api.Data;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestPowerUpdateHistoryTable : SolarDigestTableBase, ISolarDigestPowerUpdateHistoryTable
    {
        public override string TableName => Constants.Table.PowerUpdateHistory;

        public SolarDigestPowerUpdateHistoryTable(IMapper mapper, IFunctionLogger logger)
            : base(mapper, logger)
        {
        }

        public Task UpsertPowerStatusHistoryAsync(string siteId, DateTime startDateTime, DateTime endDateTime, PowerUpdateStatus status,
            CancellationToken cancellationToken = default)
        {
            var entity = new PowerUpdateHistoryEntity(siteId, startDateTime, endDateTime, status);
            return TableImpl.PutItemAsync(entity, cancellationToken);
        }
    }
}