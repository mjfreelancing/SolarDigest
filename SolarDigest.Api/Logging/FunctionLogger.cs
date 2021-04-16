using Amazon.Lambda.Core;

namespace SolarDigest.Api.Logging
{
    internal sealed class FunctionLogger : IFunctionLogger
    {
        private ILambdaLogger _lambdaLogger;

        public void LogDebug(string message)
        {
            _lambdaLogger?.Log(message);
        }

        internal void SetLambdaLogger(ILambdaLogger logger)
        {
            _lambdaLogger = logger;
        }
    }
}