using FluentValidation;
using System;
using System.Linq.Expressions;

namespace SolarDigest.Api.Validation
{
    public abstract class SolarDigestValidator<TType> : AbstractValidator<TType>
    {
        static SolarDigestValidator()
        {
            // prevent Pascal name splitting
            ValidatorOptions.Global.DisplayNameResolver = (type, info, expression) => info.Name;
        }

        protected IRuleBuilderOptions<TType, TProperty> IsRequired<TProperty>(Expression<Func<TType, TProperty>> expression)
        {
            return RuleFor(expression)
                .NotEmpty()
                .WithErrorCode($"{ValidationErrorCode.Required}");
        }

        protected IRuleBuilderOptions<TType, TProperty> IsGreaterThan<TProperty>(Expression<Func<TType, TProperty>> expression, TProperty value)
            where TProperty : IComparable<TProperty>, IComparable
        {
            return RuleFor(expression)
                .GreaterThan(value)
                .WithErrorCode($"{ValidationErrorCode.OutOfRange}");
        }

        protected IRuleBuilderOptions<TType, TProperty?> IsGreaterThan<TProperty>(Expression<Func<TType, TProperty?>> expression, TProperty value)
            where TProperty : struct, IComparable<TProperty>, IComparable
        {
            return RuleFor(expression)
                .GreaterThan(value)
                .WithErrorCode($"{ValidationErrorCode.OutOfRange}");
        }
    }
}