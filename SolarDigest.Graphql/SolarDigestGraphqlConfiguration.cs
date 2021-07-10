namespace SolarDigest.Graphql
{
    public sealed class SolarDigestGraphqlConfiguration : ISolarDigestGraphqlConfiguration
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
    }
}