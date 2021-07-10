using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SolarDigest.Api.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static void InvokeValidator<TValidator, TType>(this IServiceProvider serviceProvider, TType instance)
            where TValidator : IValidator<TType>
        {
            var validator = serviceProvider.GetRequiredService<TValidator>();
            validator.AssertValidation(instance);
        }
    }
}