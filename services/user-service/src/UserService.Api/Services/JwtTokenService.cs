using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserService.Database;
using UserService.Database.Entities;

namespace UserService.Api.Services;

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public string GenerateToken(User user)
    {
        return GenerateToken(user, out _);
    }

    public string GenerateToken(User user, out string tokenJti)
    {
        var jwtKey = configuration["Jwt:Key"] ?? EnvironmentVariables.GetRequiredVariable("JWT_SECRET_KEY");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "UserService";
        var jwtAudience = configuration["Jwt:Audience"] ?? "UserService";
        var expirationHours = int.Parse(configuration["Jwt:ExpirationHours"] ?? "24");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jti = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddHours(expirationHours);

        var claims = new[]
        {
            new Claim("user_id", user.Id.ToString()),
            new Claim("tenant_id", user.TenantId?.ToString() ?? ""),
            new Claim("role", ((int)user.Role).ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        tokenJti = jti;
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}