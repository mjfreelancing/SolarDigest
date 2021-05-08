using System.Text.Json.Serialization;

namespace SolarDigest.Api.Payloads.GraphQL
{
    // associated with a lambda resolver request
    public sealed class GetSiteInfoPayload
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}