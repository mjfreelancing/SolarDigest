using AllOverIt.Helpers;
using SolarDigest.Api.Data;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Repository;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    internal sealed class PersistExceptionHandler : IExceptionHandler
    {
        private readonly ISolarDigestExceptionTable _exceptionTable;
        private readonly IFunctionLogger _logger;

        public PersistExceptionHandler(ISolarDigestExceptionTable exceptionTable, IFunctionLogger logger)
        {
            _exceptionTable = exceptionTable.WhenNotNull(nameof(exceptionTable));
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task HandleAsync(Exception exception)
        {
            _logger.LogDebug($"Persisting an exception: {exception.Message}");

            try
            {
                var entity = new ExceptionEntity
                {
                    Message = exception.Message,
                    StackTrace = exception.StackTrace
                };

                await _exceptionTable.PutItemAsync(entity);

                _logger.LogDebug($"Exception persisted: {exception.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Failed to persist an exception event '{exception.Message}' due to {ex.Message}");
            }
        }
    }
}