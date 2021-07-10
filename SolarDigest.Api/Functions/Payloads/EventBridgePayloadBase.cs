using System;

namespace SolarDigest.Api.Functions.Payloads
{
    public abstract class EventBridgePayloadBase
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public DateTime Time { get; set; }
    }

    public abstract class EventBridgePayloadBase<TEvent> : EventBridgePayloadBase
    {
        public TEvent Detail { get; set; }
    }
}