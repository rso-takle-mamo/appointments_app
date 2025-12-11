using UserService.Database.Entities;

namespace UserService.Api.Services.Interfaces;

public interface ISessionService
{
    Task<UserSession> CreateSessionAsync(Guid userId, string tokenJti, DateTime expiresAt);
    Task<UserSession?> GetSessionByJtiAsync(string tokenJti);
    Task<bool> IsSessionActiveAsync(string tokenJti);
    Task<bool> InvalidateSessionAsync(string tokenJti);
    Task<bool> InvalidateUserSessionsAsync(Guid userId);
}