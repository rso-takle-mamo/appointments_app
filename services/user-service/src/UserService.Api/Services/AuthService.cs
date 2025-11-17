using UserService.Api.Requests;
using UserService.Api.Responses;
using UserService.Database.Entities;
using UserService.Database.Enums;
using UserService.Database.Repositories.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace UserService.Api.Services;

public class AuthService(IUserRepository userRepository, IJwtTokenService jwtTokenService, ISessionService sessionService, ITenantRepository tenantRepository)
    : IAuthService
{
    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        var isValidPassword = await userRepository.VerifyPassword(request.Username, request.Password);

        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        var user = await userRepository.GetByUsername(request.Username);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        await sessionService.InvalidateUserSessionsAsync(user.Id);

        var token = jwtTokenService.GenerateToken(user, out var tokenJti);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        await sessionService.CreateSessionAsync(user.Id, tokenJti, expiresAt);

        return new TokenResponse
        {
            AccessToken = token,
            ExpiresIn = expiresAt
        };
    }

    public async Task<TokenResponse> RegisterAsync(RegisterRequest request)
    {
        var usernameExists = await userRepository.UsernameExists(request.Username);
        if (usernameExists)
        {
            throw new InvalidOperationException("Username already exists");
        }

        if (request.TenantId.HasValue && !await tenantRepository.Exists(request.TenantId.Value))
        {
            throw new InvalidOperationException("Tenant does not exist");
        }

        var role = request.TenantId.HasValue ? UserRole.Provider : UserRole.Customer;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Role = role,
            TenantId = request.TenantId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await userRepository.Create(user);

        var token = jwtTokenService.GenerateToken(user, out var tokenJti);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        await sessionService.CreateSessionAsync(user.Id, tokenJti, expiresAt);

        return new TokenResponse
        {
            AccessToken = token,
            ExpiresIn = expiresAt
        };
    }

    public async Task LogoutAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Invalid token format");
        }

        var actualToken = token.StartsWith("Bearer ")
            ? token.Replace("Bearer ", "")
            : token;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(actualToken);
            var tokenJti = jsonToken.Id;

            if (!string.IsNullOrEmpty(tokenJti))
            {
                var invalidated = await sessionService.InvalidateSessionAsync(tokenJti);
                if (!invalidated)
                {
                    throw new InvalidOperationException("Session not found or already invalidated");
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid token: missing JTI claim");
            }
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException("Invalid token format", ex);
        }
        catch (System.Exception ex)
        {
            throw new InvalidOperationException("Failed to process token during logout", ex);
        }
    }
}