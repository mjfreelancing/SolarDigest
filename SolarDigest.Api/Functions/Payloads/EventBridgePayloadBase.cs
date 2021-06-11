using System;

namespace SolarDigest.Api.Functions.Payloads
{
    public class EventBridgePayloadBase
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public DateTime Time { get; set; }
    }

    public class EventBridgePayloadBase<TEvent> : EventBridgePayloadBase
    {
        public TEvent Detail { get; set; }
    }
}