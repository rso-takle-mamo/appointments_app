using UserService.Api.Requests;
using UserService.Api.Responses;

namespace UserService.Api.Services;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request);
    Task<TokenResponse> RegisterAsync(RegisterRequest request);
    Task LogoutAsync(string token);
}