using AllOverIt.Extensions;
using AllOverIt.Helpers;
using FluentValidation.Results;
using SolarDigest.Api.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SolarDigest.Api.Exceptions
{
    [Serializable]
    public sealed class SolarDigestValidationException : Exception
    {
        private readonly string _message;
        public override string Message => _message ?? base.Message;

        public IEnumerable<ValidationError> Errors { get; }

        public SolarDigestValidationException(IEnumerable<ValidationError> errors)
        {
            Errors = errors
                .WhenNotNullOrEmpty(nameof(errors))
                .AsReadOnlyCollection();

            if (Errors.Any())
            {
                _message = string.Join(", ", Errors.Select(error => error.Message));
            }
        }

        public SolarDigestValidationException(IEnumerable<ValidationFailure> failures)
            : this(CreateValidationErrors(failures))
        {
        }

        [ExcludeFromCodeCoverage]
        // Constructor should be protected for unsealed classes, private for sealed classes.
        // (The Serializer invokes this constructor through reflection, so it can be private)
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        private SolarDigestValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var errorsInfo = info.GetValue(nameof(Errors), typeof(IEnumerable<ValidationError>)) ?? Enumerable.Empty<ValidationError>();
            Errors = errorsInfo as IEnumerable<ValidationError>;
        }

        [ExcludeFromCodeCoverage]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _ = info.WhenNotNull(nameof(info));

            info.AddValue(nameof(Errors), Errors);
            base.GetObjectData(info, context);
        }

        private static IEnumerable<ValidationError> CreateValidationErrors(IEnumerable<ValidationFailure> failures)
        {
            return failures
                .WhenNotNull(nameof(failures))
                .Select(item => new ValidationError(
                    item.ErrorCode.As<ValidationErrorCode>(),
                    item.ErrorMessage,
                    item.PropertyName,
                    item.AttemptedValue))
                .AsReadOnlyCollection();
        }
    }
}