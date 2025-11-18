using UserService.Database.Entities;

namespace UserService.Database.Repositories.Interfaces;

public interface IUserSessionRepository
{
    Task<UserSession?> GetByJtiAsync(string tokenJti);
    Task<UserSession> CreateAsync(UserSession session);
    Task<bool> InvalidateByJtiAsync(string tokenJti);
    Task<bool> InvalidateByUserIdAsync(Guid userId);
}