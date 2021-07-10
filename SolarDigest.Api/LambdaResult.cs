using AllOverIt.Extensions;
using SolarDigest.Api.Exceptions;
using SolarDigest.Api.Validation;
using System;
using System.Collections.Generic;

namespace SolarDigest.Api
{
    public sealed class LambdaResult<TResultType>
    {
        public bool Success { get; }
        public TResultType Payload { get; }
        public IEnumerable<ValidationError> ValidationErrors { get; }
        public string Error { get; }

        public LambdaResult(TResultType payload)
        {
            Success = true;
            Payload = payload;
        }

        public LambdaResult(Exception exception)
        {
            Success = false;

            if (exception is SolarDigestValidationException validationException)
            {
                ValidationErrors = validationException.Errors.AsReadOnlyCollection();
            }

            Error = exception.Message;
        }
    }
}