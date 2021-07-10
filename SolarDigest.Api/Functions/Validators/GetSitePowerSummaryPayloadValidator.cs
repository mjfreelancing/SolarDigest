using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Validation;
using SolarDigest.Models;

namespace SolarDigest.Api.Functions.Validators
{
    // IValidator<GetSitePowerSummaryPayload>
    public sealed class GetSitePowerSummaryPayloadValidator : SolarDigestValidator<GetSitePowerSummaryPayload>
    {
        public GetSitePowerSummaryPayloadValidator()
        {
            // only really required for when invoked via the console test area since these are always validated by AppSync
            IsRequired(model => model.SiteId);
            IsValidDate(model => model.StartDate, Constants.DateFormat);
            IsValidDate(model => model.EndDate, Constants.DateFormat);
            IsValidEnum<MeterType>(model => model.MeterType);
            IsValidEnum<SummaryType>(model => model.SummaryType);
        }
    }
}