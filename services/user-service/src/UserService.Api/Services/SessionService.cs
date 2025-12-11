using UserService.Api.Services.Interfaces;
using UserService.Database.Entities;
using UserService.Database.Repositories.Interfaces;

namespace UserService.Api.Services;

public class SessionService(IUserSessionRepository sessionRepository) : ISessionService
{
    public async Task<UserSession> CreateSessionAsync(Guid userId, string tokenJti, DateTime expiresAt)
    {
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenJti = tokenJti,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };

        return await sessionRepository.CreateAsync(session);
    }

    public async Task<UserSession?> GetSessionByJtiAsync(string tokenJti)
    {
        return await sessionRepository.GetByJtiAsync(tokenJti);
    }

    public async Task<bool> IsSessionActiveAsync(string tokenJti)
    {
        var session = await sessionRepository.GetByJtiAsync(tokenJti);
        return session != null && session.ExpiresAt > DateTime.UtcNow;
    }

    public async Task<bool> InvalidateSessionAsync(string tokenJti)
    {
        return await sessionRepository.InvalidateByJtiAsync(tokenJti);
    }

    public async Task<bool> InvalidateUserSessionsAsync(Guid userId)
    {
        return await sessionRepository.InvalidateByUserIdAsync(userId);
    }
}