using SolarDigest.Api.Validation;
using SolarDigest.Models;

namespace SolarDigest.Api.Functions
{
    public sealed class SiteDetailsValidator : SolarDigestValidator<SiteDetails>
    {
        public SiteDetailsValidator()
        {
            // minimum fields required for a site being added / updated
            IsRequired(model => model.StartDate);
            IsRequired(model => model.ApiKey);
            IsRequired(model => model.ContactEmail);
            IsRequired(model => model.TimeZoneId);
        }
    }
}