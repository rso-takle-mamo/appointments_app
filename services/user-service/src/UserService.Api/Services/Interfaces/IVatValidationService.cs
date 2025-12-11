using UserService.Api.Responses;

namespace UserService.Api.Services.Interfaces;

public interface IVatValidationService
{
    Task<VatValidationResponse> ValidateVatAsync(string vatNumber);
}