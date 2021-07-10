using System;
using System.Runtime.Serialization;

namespace SolarDigest.Api.Exceptions
{
    [Serializable]
    public sealed class SolarEdgeTimeoutException : SolarEdgeExceptionBase
    {

        public SolarEdgeTimeoutException()
        {
        }

        public SolarEdgeTimeoutException(string message)
            : base(message)
        {
        }

        public SolarEdgeTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SolarEdgeTimeoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public SolarEdgeTimeoutException(string siteId, string startDateTime, string endDateTime)
            : base(siteId, startDateTime, endDateTime, "SolarEdge request timed out")
        {
        }
    }
}