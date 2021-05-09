using AllOverIt.Helpers;
using SolarDigest.Api.Data;
using SolarDigest.Api.Logging;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    internal sealed class PersistExceptionHandler : IExceptionHandler
    {
        private readonly ISolarDigestDynamoDb _dynamoDb;
        private readonly IFunctionLogger _logger;

        public PersistExceptionHandler(ISolarDigestDynamoDb dynamoDb, IFunctionLogger logger)
        {
            _dynamoDb = dynamoDb.WhenNotNull(nameof(dynamoDb));
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

                await _dynamoDb.PutItemAsync(Constants.Table.Exception, entity);

                _logger.LogDebug($"Exception persisted: {exception.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Failed to persist an exception event '{exception.Message}' due to {ex.Message}");
            }
        }
    }
}