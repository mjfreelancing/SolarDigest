using SolarDigest.Api.Validation;
using SolarDigest.Models;

namespace SolarDigest.Api.Functions
{
    // For the purpose of CDK deployment, all functions need to reside in the same (SolarDigest.Api.Functions) namespace.

    public sealed class SiteDetailsValidator : SolarDigestValidator<SiteDetails>
    {
        public SiteDetailsValidator()
        {
            // minimum fields required for a site being added / updated
            IsRequired(model => model.StartDate);
            IsRequired(model => model.ContactName);
            IsRequired(model => model.ContactEmail);
            IsRequired(model => model.TimeZoneId);
        }
    }
}