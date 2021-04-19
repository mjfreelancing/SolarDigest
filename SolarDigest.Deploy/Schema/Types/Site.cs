using SolarDigest.Deploy.Attributes;

namespace SolarDigest.Deploy.Schema.Types
{
    // interpreted as an output type
    internal interface Site
    {
        [GraphqlTypeRequired]
        public int Id { get; }

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

        public string LastAggregationDate { get; }

        public string LastSummaryDate { get; }

        public string LastRefreshDateTime { get; }
    }
}