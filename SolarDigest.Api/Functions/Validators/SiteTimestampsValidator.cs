using SolarDigest.Api.Validation;
using SolarDigest.Models;

namespace SolarDigest.Api.Functions.Validators
{
    public sealed class SiteTimestampsValidator : SolarDigestValidator<SiteTimestamps>
    {
        //private const string DateFormat = "yyyy-MM-dd";
        //private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public SiteTimestampsValidator()
        {
            IsValidDate(model => model.LastSummaryDate, Constants.DateFormat);
            IsValidDate(model => model.LastAggregationDate, Constants.DateFormat);
            IsValidDateTime(model => model.LastRefreshDateTime, Constants.DateTimeFormat);
        }
    }
}