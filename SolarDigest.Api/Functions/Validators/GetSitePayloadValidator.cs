using SolarDigest.Api.Functions.Payloads;
using SolarDigest.Api.Validation;

namespace SolarDigest.Api.Functions.Validators
{
    // IValidator<GetSitePayload>
    public sealed class GetSitePayloadValidator : SolarDigestValidator<GetSitePayload>
    {
        public GetSitePayloadValidator()
        {
            IsRequired(model => model.Id);
        }
    }
}