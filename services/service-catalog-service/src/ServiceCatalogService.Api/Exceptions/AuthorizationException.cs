namespace ServiceCatalogService.Api.Exceptions;

public class AuthorizationException : BaseDomainException
{
    public override string ErrorCode => "ACCESS_DENIED";

    public string Resource { get; }

    public string Action { get; }

    public AuthorizationException(string message) : base(message) { }

    public AuthorizationException(string resource, string action, string message) : base(message)
    {
        Resource = resource;
        Action = action;
    }

    public AuthorizationException(string message, Exception innerException) : base(message, innerException) { }

    public AuthorizationException(string resource, string action, string message, Exception innerException) : base(message, innerException)
    {
        Resource = resource;
        Action = action;
    }
}