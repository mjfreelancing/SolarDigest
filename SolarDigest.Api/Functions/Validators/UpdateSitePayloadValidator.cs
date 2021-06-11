using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Validation;

namespace SolarDigest.Api.Functions.Validators
{
    // IValidator<UpdateSitePayload>
    public sealed class UpdateSitePayloadValidator : SolarDigestValidator<UpdateSitePayload>
    {
        public UpdateSitePayloadValidator()
        {
            IsRequired(model => model.Id);
            IsRequired(model => model.Site);
            RuleFor(model => model.Site).SetValidator(new SiteDetailsValidator());
            RuleFor(model => model.Timestamps).SetValidator(new SiteTimestampsValidator());
        }
    }
}