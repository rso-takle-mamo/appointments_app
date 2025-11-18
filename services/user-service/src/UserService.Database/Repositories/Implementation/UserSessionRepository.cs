using Microsoft.EntityFrameworkCore;
using UserService.Database.Entities;
using UserService.Database.Repositories.Interfaces;

namespace UserService.Database.Repositories.Implementation;

internal class UserSessionRepository(UserDbContext context) : IUserSessionRepository
{
    public async Task<UserSession?> GetByJtiAsync(string tokenJti)
    {
        return await context.UserSessions
            .FirstOrDefaultAsync(s => s.TokenJti == tokenJti);
    }

    public async Task<UserSession> CreateAsync(UserSession session)
    {
        await context.UserSessions.AddAsync(session);
        await context.SaveChangesAsync();
        return session;
    }

    // Unused methods removed:
    // public async Task<UserSession?> UpdateAsync(UserSession session)
    // public async Task<IEnumerable<UserSession>> GetExpiredSessionsAsync(DateTime beforeDate)
    // public async Task<int> DeleteExpiredSessionsAsync(DateTime beforeDate)

    public async Task<bool> InvalidateByJtiAsync(string tokenJti)
    {
        var session = await context.UserSessions
            .FirstOrDefaultAsync(s => s.TokenJti == tokenJti);

        if (session == null) return false;

        context.UserSessions.Remove(session);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> InvalidateByUserIdAsync(Guid userId)
    {
        var sessions = await context.UserSessions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        context.UserSessions.RemoveRange(sessions);
        await context.SaveChangesAsync();
        return sessions.Count > 0;
    }
}