using AutoMapper;
using SolarDigest.Api.Data;
using SolarDigest.Api.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    internal sealed class SolarDigestExceptionTable : SolarDigestTableBase, ISolarDigestExceptionTable
    {
        public override string TableName => Constants.Table.Exception;

        public SolarDigestExceptionTable(IMapper mapper, IFunctionLogger logger)
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