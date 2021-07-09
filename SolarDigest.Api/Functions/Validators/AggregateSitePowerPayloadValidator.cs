using SolarDigest.Api.Events;
using SolarDigest.Api.Validation;

namespace SolarDigest.Api.Functions.Validators
{
    // AggregateSitePowerPayload

    public sealed class AggregateSitePowerPayloadValidator : SolarDigestValidator<AggregateSitePowerEvent>
    {
        public AggregateSitePowerPayloadValidator()
        {
            IsRequired(model => model.SiteId);
            IsValidDate(model => model.StartDate, Constants.DateFormat);
            IsValidDate(model => model.EndDate, Constants.DateFormat);

            // really should validate the date range is valid
        }
    }
}