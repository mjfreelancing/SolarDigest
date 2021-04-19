using AllOverIt.Helpers;
using Amazon.DynamoDBv2;
using SolarDigest.Api.Data;
using SolarDigest.Api.Extensions;
using SolarDigest.Api.Logging;
using System;
using System.Threading.Tasks;

namespace SolarDigest.Api
{
    internal sealed class FunctionExceptionHandler : IExceptionHandler
    {
        private readonly IFunctionLogger _logger;

        public FunctionExceptionHandler(IFunctionLogger logger)
        {
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task HandleAsync(Exception exception)
        {
            _logger.LogDebug($"Received an exception to persist: {exception.Message}");

            try
            {
                var dbClient = new AmazonDynamoDBClient();

                var entity = new ExceptionEntity
                {
                    Message = exception.Message,
                    StackTrace = exception.StackTrace
                };

                await dbClient.PutItemAsync("Exception", entity);

                _logger.LogDebug($"Exception persisted: {exception.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Failed to persist an exception event '{exception.Message}' due to {ex.Message}");
            }
        }
    }
}