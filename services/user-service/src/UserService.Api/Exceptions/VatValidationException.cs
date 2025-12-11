namespace UserService.Api.Exceptions;

public class VatValidationException : Exception
{
    public string VatNumber { get; }
    
    public string ErrorCode { get; }

    public VatValidationException(string vatNumber, string message, string errorCode = "VAT_VALIDATION_ERROR")
        : base(message)
    {
        VatNumber = vatNumber;
        ErrorCode = errorCode;
    }

    public VatValidationException(string vatNumber, string message, Exception innerException, string errorCode = "VAT_VALIDATION_ERROR")
        : base(message, innerException)
    {
        VatNumber = vatNumber;
        ErrorCode = errorCode;
    }
}