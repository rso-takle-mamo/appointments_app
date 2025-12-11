namespace UserService.Api.Responses;

public class VatValidationResponse
{
    public bool IsValid { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
}