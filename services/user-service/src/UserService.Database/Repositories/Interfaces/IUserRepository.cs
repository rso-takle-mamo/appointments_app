using UserService.Database.Entities;

namespace UserService.Database.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> Get(Guid id);
    Task Create(User user);
    Task<User?> GetByUsername(string username);
    Task<bool> UsernameExists(string username);
    Task<bool> VerifyPassword(string username, string password);
}