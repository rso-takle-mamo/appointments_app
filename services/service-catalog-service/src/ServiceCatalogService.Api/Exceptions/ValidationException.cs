using ServiceCatalogService.Api.Models;

namespace ServiceCatalogService.Api.Exceptions;

public class ValidationException : BaseDomainException
{
    public override string ErrorCode => "VALIDATION_ERROR";

    public List<ValidationError> ValidationErrors { get; }

    public ValidationException(List<ValidationError> validationErrors)
        : base($"Validation failed with {validationErrors.Count} error(s).")
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string message) : base(message)
    {
        ValidationErrors = new List<ValidationError>();
    }

    public ValidationException(string message, List<ValidationError> validationErrors)
        : base(message)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
        ValidationErrors = new List<ValidationError>();
    }
}