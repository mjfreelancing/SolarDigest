using AllOverIt.Helpers;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SolarDigest.Api.Exceptions
{
    public abstract class SolarEdgeExceptionBase : Exception
    {
        public string SiteId { get; }
        public string StartDateTime { get; }
        public string EndDateTime { get; }

        protected SolarEdgeExceptionBase(string siteId, string startDateTime, string endDateTime, string message)
            : this(message)
        {
            SiteId = siteId;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }

        protected SolarEdgeExceptionBase()
        {
        }

        protected SolarEdgeExceptionBase(string message)
            : base(message)
        {
        }

        protected SolarEdgeExceptionBase(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        // Constructor should be protected for unsealed classes, private for sealed classes.
        // (The Serializer invokes this constructor through reflection, so it can be private)
        protected SolarEdgeExceptionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            SiteId = info.GetString(nameof(SiteId));
            StartDateTime = info.GetString(nameof(StartDateTime));
            EndDateTime = info.GetString(nameof(EndDateTime));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _ = info.WhenNotNull(nameof(info));

            base.GetObjectData(info, context);

            info.AddValue(nameof(SiteId), SiteId);
            info.AddValue(nameof(StartDateTime), StartDateTime);
            info.AddValue(nameof(EndDateTime), EndDateTime);
        }
    }
}