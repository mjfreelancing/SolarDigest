using AllOverIt.Helpers;
using FluentValidation;
using SolarDigest.Api.Exceptions;

namespace SolarDigest.Api.Validation.Extensions
{
    public static class ValidatorExtensions
    {
        public static void AssertValidation<TType>(this IValidator<TType> validator, TType instance)
        {
            var validationResult = validator
                .WhenNotNull(nameof(validator))
                .Validate(instance);

            if (!validationResult.IsValid)
            {
                throw new SolarDigestValidationException(validationResult.Errors);
            }
        }
    }
}