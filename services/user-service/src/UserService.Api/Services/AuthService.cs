using Microsoft.EntityFrameworkCore;
using UserService.Api.Requests;
using UserService.Api.Responses;
using UserService.Api.Exceptions;
using UserService.Database;
using UserService.Database.Entities;
using UserService.Database.Enums;
using UserService.Database.Repositories.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace UserService.Api.Services;

public class AuthService(IUserRepository userRepository, IJwtTokenService jwtTokenService, ISessionService sessionService, ITenantRepository tenantRepository, UserDbContext dbContext)
    : IAuthService
{
    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        var isValidPassword = await userRepository.VerifyPassword(request.Username, request.Password);

        if (!isValidPassword)
        {
            throw new AuthenticationException("login", "Invalid username or password");
        }

        var user = await userRepository.GetByUsername(request.Username);
        if (user == null)
        {
            throw new AuthenticationException("login", "Invalid username or password");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            await sessionService.InvalidateUserSessionsAsync(user.Id);

            await transaction.CommitAsync();

            var token = jwtTokenService.GenerateToken(user, out var tokenJti);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            await sessionService.CreateSessionAsync(user.Id, tokenJti, expiresAt);

            return new TokenResponse
            {
                AccessToken = token,
                ExpiresIn = expiresAt
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task LogoutAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ValidationException("Invalid token format");
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
                    throw new DatabaseOperationException("invalidate", "Session", "Session not found or already invalidated");
                }
            }
            else
            {
                throw new ValidationException("Invalid token: missing JTI claim");
            }
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DatabaseOperationException("process", "Token", "Failed to process token during logout", ex);
        }
    }
    
    public async Task<TokenResponse> RegisterCustomerAsync(CustomerRegisterRequest request)
    {
        var usernameExists = await userRepository.UsernameExists(request.Username);
        if (usernameExists)
        {
            throw new ConflictException("username", "Username already exists");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Role = UserRole.Customer,
                TenantId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await userRepository.Create(user);

            await transaction.CommitAsync();

            var token = jwtTokenService.GenerateToken(user, out var tokenJti);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            await sessionService.CreateSessionAsync(user.Id, tokenJti, expiresAt);

            return new TokenResponse
            {
                AccessToken = token,
                ExpiresIn = expiresAt
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<TokenResponse> RegisterProviderAsync(ProviderRegisterRequest request)
    {
        var usernameExists = await userRepository.UsernameExists(request.Username);
        if (usernameExists)
        {
            throw new ConflictException("username", "Username already exists");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Role = UserRole.Provider,
                TenantId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await userRepository.Create(user);

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                OwnerId = user.Id,
                BusinessName = request.BusinessName,
                BusinessEmail = request.BusinessEmail,
                BusinessPhone = request.BusinessPhone,
                Address = request.Address,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await tenantRepository.CreateAsync(tenant);

            user.TenantId = tenant.Id;
            user.UpdatedAt = DateTime.UtcNow;
            await userRepository.UpdateAsync(user);

            await transaction.CommitAsync();

            var token = jwtTokenService.GenerateToken(user, out var tokenJti);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            await sessionService.CreateSessionAsync(user.Id, tokenJti, expiresAt);

            return new TokenResponse
            {
                AccessToken = token,
                ExpiresIn = expiresAt
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}