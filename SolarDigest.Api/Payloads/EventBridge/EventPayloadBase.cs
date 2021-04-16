using System;

namespace SolarDigest.Api.Payloads.EventBridge
{
    public class EventPayloadBase
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public DateTime Time { get; set; }
    }

    public class EventPayloadBase<TEvent> : EventPayloadBase
    {
        public TEvent Detail { get; set; }
    }
}