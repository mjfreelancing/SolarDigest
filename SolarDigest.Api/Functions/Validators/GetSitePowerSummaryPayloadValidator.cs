using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Models;
using SolarDigest.Api.Validation;
using SolarDigest.Models;

namespace SolarDigest.Api.Functions.Validators
{
    // IValidator<GetSitePowerSummaryPayload>
    public sealed class GetSitePowerSummaryPayloadValidator : SolarDigestValidator<GetSitePowerSummaryPayload>
    {
        public GetSitePowerSummaryPayloadValidator()
        {
            IsRequired(model => model.SiteId);
            IsValidDate(model => model.StartDate, Constants.DateFormat);
            IsValidDate(model => model.EndDate, Constants.DateFormat);
            IsValidEnum<MeterType>(model => model.MeterType);
            IsValidEnum<SummaryType>(model => model.SummaryType);
        }
    }
}