using AllOverIt.Aws.Cdk.AppSync.Attributes;

namespace SolarDigest.Deploy.Schema.Inputs
{
    [GraphqlInput]
    internal interface SiteInput
    {
        [GraphqlTypeRequired]
        public string StartDate { get; }

        [GraphqlTypeRequired]
        public string ApiKey { get; }

        [GraphqlTypeRequired]
        public string ContactName { get; }

        [GraphqlTypeRequired]
        public string ContactEmail { get; }

        [GraphqlTypeRequired]
        public string TimeZoneId { get; }
    }
}