using SolarDigest.Api.Validation;

namespace SolarDigest.Api.Functions.AddSite
{
    // IValidator<AddSitePayload>
    public sealed class AddSitePayloadValidator : SolarDigestValidator<AddSitePayload>
    {
        public AddSitePayloadValidator()
        {
            IsRequired(model => model.Id);
            IsRequired(model => model.Site);

            // todo: define a validator for the Site and set the configurator for that property
        }
    }
}