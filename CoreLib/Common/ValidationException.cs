using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib.Models;

namespace CoreLib.Exceptions
{
    public class ValidationException : DatabaseException
    {
        public ValidationResult ValidationResult { get; }

        public ValidationException(ValidationResult validationResult) 
            : base($"Validation failed: {string.Join(", ", validationResult.Errors)}")
        {
            ValidationResult = validationResult;
        }

        public ValidationException(string error) 
            : this(ValidationResult.Failure(error))
        {
        }
    }
}