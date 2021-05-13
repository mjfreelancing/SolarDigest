using System;

namespace SolarDigest.Api
{
    public sealed class LambdaResult<TResultType>
    {
        public bool Success { get; }
        public TResultType Result { get; }
        public string Error { get; }        // todo: consider changing this to provide validation / exception errors

        public LambdaResult(TResultType result)
        {
            Success = true;
            Result = result;
        }

        public LambdaResult(Exception exception)
        {
            Success = false;
            Error = exception.Message;
        }
    }
}