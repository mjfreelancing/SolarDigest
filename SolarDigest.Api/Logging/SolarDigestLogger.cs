using Amazon.Lambda.Core;
using System;

namespace SolarDigest.Api.Logging
{
    internal sealed class SolarDigestLogger : ISolarDigestLogger
    {
        private ILambdaLogger _lambdaLogger;

        public void LogDebug(string message)
        {
            _lambdaLogger?.Log(message);
        }

        public void LogException(Exception exception)
        {
            _lambdaLogger?.Log($"ERR: {exception.Message}");
        }

        internal void SetLambdaLogger(ILambdaLogger logger)
        {
            _lambdaLogger = logger;
        }
    }
}