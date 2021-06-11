using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Validation;

namespace SolarDigest.Api.Functions.Validators
{
    // IValidator<AddSitePayload>
    public sealed class AddSitePayloadValidator : SolarDigestValidator<AddSitePayload>
    {
        public AddSitePayloadValidator()
        {
            IsRequired(model => model.Id);
            IsRequired(model => model.Site);
            RuleFor(model => model.Site).SetValidator(new SiteDetailsValidator());
        }
    }
}