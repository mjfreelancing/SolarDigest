using AutoMapper;
using SolarDigest.Api.Data;
using SolarDigest.Api.Logging;
using SolarDigest.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestExceptionTable : SolarDigestTableBase, ISolarDigestExceptionTable
    {
        public override string TableName => $"{Helpers.GetAppVersionName()}_{Shared.Constants.Table.Exception}";

        public SolarDigestExceptionTable(IMapper mapper, ISolarDigestLogger logger)
            : base(mapper, logger)
        {
        }

        public Task AddExceptionAsync(Exception exception, CancellationToken cancellationToken)
        {
            var entity = new ExceptionEntity
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace
            };

            return TableImpl.PutItemAsync(entity, cancellationToken);
        }
    }
}