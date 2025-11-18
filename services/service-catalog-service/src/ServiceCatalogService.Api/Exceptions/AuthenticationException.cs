namespace ServiceCatalogService.Api.Exceptions;

public class AuthenticationException : BaseDomainException
{
    public override string ErrorCode => "AUTHENTICATION_FAILED";

    public string AuthenticationType { get; }

    public AuthenticationException(string message) : base(message) { }

    public AuthenticationException(string authenticationType, string message) : base(message)
    {
        AuthenticationType = authenticationType;
    }

    public AuthenticationException(string message, Exception innerException) : base(message, innerException) { }

    public AuthenticationException(string authenticationType, string message, Exception innerException) : base(message, innerException)
    {
        AuthenticationType = authenticationType;
    }
}