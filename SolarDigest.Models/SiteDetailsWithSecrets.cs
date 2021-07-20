namespace SolarDigest.Models
{
    public sealed class SiteDetailsWithSecrets : SiteDetails, ISiteSecrets
    {
        public string ApiKey { get; set; }
    }
}