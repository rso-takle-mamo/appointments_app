using UserService.Api.Requests;
using UserService.Api.Responses;

namespace UserService.Api.Services;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request);
    Task<TokenResponse> RegisterCustomerAsync(CustomerRegisterRequest request);
    Task<TokenResponse> RegisterProviderAsync(ProviderRegisterRequest request);
    Task LogoutAsync(string token);
}