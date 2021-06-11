using AllOverIt.Helpers;
using System;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SolarDigest.Api.Exceptions
{
    [Serializable]
    public sealed class SolarEdgeResponseException : SolarEdgeExceptionBase
    {
        public HttpStatusCode StatusCode { get; }

        public SolarEdgeResponseException()
        {
        }

        public SolarEdgeResponseException(string message)
            : base(message)
        {
        }

        public SolarEdgeResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        // Constructor should be protected for unsealed classes, private for sealed classes.
        // (The Serializer invokes this constructor through reflection, so it can be private)
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        private SolarEdgeResponseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StatusCode = (HttpStatusCode)info.GetValue(nameof(StatusCode), typeof(HttpStatusCode));
        }

        public SolarEdgeResponseException(HttpStatusCode statusCode, string siteId, string startDateTime, string endDateTime)
            : base(siteId, startDateTime, endDateTime, $"SolarEdge request failed with StatusCode {(int)statusCode} ({statusCode})")
        {
            StatusCode = statusCode;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _ = info.WhenNotNull(nameof(info));

            base.GetObjectData(info, context);

            info.AddValue(nameof(StatusCode), StatusCode, typeof(HttpStatusCode));
        }
    }
}