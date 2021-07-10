using System;

namespace SolarDigest.Api.Exceptions
{
    public sealed class DynamoDbConflictException : Exception
    {
        public DynamoDbConflictException(string message)
            : base(message)
        {
        }
    }
}