using SolarDigest.Api.Validation;
using SolarDigest.Models;

namespace SolarDigest.Api.Functions
{
    public sealed class SiteTimestampsValidator : SolarDigestValidator<SiteTimestamps>
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public SiteTimestampsValidator()
        {
            IsValidDate(model => model.LastSummaryDate, DateFormat);
            IsValidDate(model => model.LastAggregationDate, DateFormat);
            IsValidDateTime(model => model.LastRefreshDateTime, DateTimeFormat);
        }
    }
}