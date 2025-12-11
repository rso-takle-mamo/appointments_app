using UserService.Api.Responses;
using UserService.Api.Exceptions;
using VatCheckApi;
using System.Text.Json;
using UserService.Api.Services.Interfaces;

namespace UserService.Api.Services;

public class VatValidationService : IVatValidationService
{
    private readonly VatCheckApiClient _vatCheckClient;
    private readonly ILogger<VatValidationService> _logger;

    public VatValidationService(ILogger<VatValidationService> logger)
    {
        var apiKey = Environment.GetEnvironmentVariable("VATCHECK_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("VATCHECK_API_KEY environment variable is not set.");
        }

        _vatCheckClient = new VatCheckApiClient(apiKey: apiKey);
        _logger = logger;
    }

    public async Task<VatValidationResponse> ValidateVatAsync(string vatNumber)
    {
        var originalVatNumber = vatNumber;

        try
        {
            _logger.LogInformation("Validating VAT number: {VatNumber}", vatNumber);

            if (string.IsNullOrWhiteSpace(vatNumber) || vatNumber.Length < 3)
            {
                _logger.LogWarning("Invalid VAT number format: {VatNumber}", vatNumber);
                return new VatValidationResponse
                {
                    IsValid = false,
                    VatNumber = vatNumber
                };
            }

            vatNumber = vatNumber.Trim().ToUpper().Replace(" ", "").Replace(".", "");

            if (vatNumber.Length < 3 || !vatNumber[..2].All(char.IsLetter) || !vatNumber[2..].All(char.IsDigit))
            {
                _logger.LogWarning("Invalid VAT number format. Expected format: CC followed by numbers only. Got: {VatNumber}", vatNumber);
                return new VatValidationResponse
                {
                    IsValid = false,
                    VatNumber = vatNumber
                };
            }

            var requestParams = new Dictionary<string, string?>
            {
                { "vat_number", vatNumber }
            };

            var validationResult = await _vatCheckClient.CheckAsync(requestParams);

            var formatValidValue = false;
            var checksumValidValue = false;

            var formatValid = validationResult.TryGetValue("format_valid", out var formatValidObj) &&
                            formatValidObj != null && bool.TryParse(formatValidObj.ToString(), out formatValidValue) && formatValidValue;

            var checksumValid = validationResult.TryGetValue("checksum_valid", out var checksumValidObj) &&
                               checksumValidObj != null && bool.TryParse(checksumValidObj.ToString(), out checksumValidValue) && checksumValidValue;

            var isValid = formatValid && checksumValid;

            if (!isValid)
            {
                _logger.LogWarning("Invalid VAT number: {VatNumber} (FormatValid: {FormatValid}, ChecksumValid: {ChecksumValid})",
                    vatNumber, formatValidValue, checksumValidValue);
                return new VatValidationResponse
                {
                    IsValid = false,
                    VatNumber = vatNumber
                };
            }

            // Check if the company is registered
            var companyName = string.Empty;
            var address = string.Empty;
            var countryCode = string.Empty;

            if (validationResult.TryGetValue("registration_info", out var registrationInfoObj) &&
                registrationInfoObj is JsonElement registrationInfoElement)
            {
                // Extract company details from registration_info
                if (registrationInfoElement.TryGetProperty("name", out var nameElement))
                {
                    companyName = nameElement.GetString() ?? string.Empty;
                }

                if (registrationInfoElement.TryGetProperty("address", out var addressElement))
                {
                    address = addressElement.GetString() ?? string.Empty;
                }

                if (registrationInfoElement.TryGetProperty("is_registered", out var isRegisteredElement) &&
                    (!isRegisteredElement.GetBoolean() || string.IsNullOrEmpty(companyName)))
                {
                    _logger.LogWarning("VAT number {VatNumber} is not registered or has no company name", vatNumber);
                    return new VatValidationResponse
                    {
                        IsValid = false,
                        VatNumber = vatNumber
                    };
                }
            }

            if (validationResult.TryGetValue("country_code", out var countryCodeObj))
            {
                countryCode = countryCodeObj?.ToString() ?? string.Empty;
            }

            if (validationResult.TryGetValue("vat_number", out var normalizedVatObj))
            {
                var normalizedVat = normalizedVatObj?.ToString();
                if (!string.IsNullOrEmpty(normalizedVat))
                {
                    vatNumber = normalizedVat;
                }
            }

            _logger.LogInformation("VAT number {VatNumber} is valid for company: {CompanyName} in {CountryCode}",
                vatNumber, companyName, countryCode);

            return new VatValidationResponse
            {
                IsValid = true,
                CompanyName = companyName,
                Address = address,
                CountryCode = countryCode,
                VatNumber = vatNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating VAT number: {VatNumber}", originalVatNumber);
            throw new VatValidationException(
                originalVatNumber,
                "Unable to validate VAT number at this time. Please try again later.",
                ex,
                "VAT_SERVICE_ERROR"
            );
        }
    }
}