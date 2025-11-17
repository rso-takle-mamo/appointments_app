using UserService.Database.Entities;

namespace UserService.Api.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateToken(User user, out string tokenJti);
}