using UserService.Api.Models;

namespace UserService.Api.Exceptions;

public class ValidationException : BaseDomainException
{
    public override string ErrorCode => "VALIDATION_ERROR";

    public Dictionary<string, List<ValidationError>> ValidationErrors { get; }

    public ValidationException(string message, Dictionary<string, List<ValidationError>> validationErrors)
        : base(message)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string message) : base(message)
    {
        ValidationErrors = new Dictionary<string, List<ValidationError>>();
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
        ValidationErrors = new Dictionary<string, List<ValidationError>>();
    }
}