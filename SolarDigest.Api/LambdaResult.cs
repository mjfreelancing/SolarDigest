using System;

namespace SolarDigest.Api
{
    public sealed class LambdaResult<TResultType>
    {
        public bool Success { get; }
        public TResultType Payload { get; }
        public string Error { get; }        // todo: consider changing this to provide validation / exception errors

        public LambdaResult(TResultType payload)
        {
            Success = true;
            Payload = payload;
        }

        public LambdaResult(Exception exception)
        {
            Success = false;
            Error = exception.Message;
        }
    }
}