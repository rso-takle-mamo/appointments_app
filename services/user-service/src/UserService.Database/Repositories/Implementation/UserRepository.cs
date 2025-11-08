using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using UserService.Database.Entities;
using UserService.Database.Repositories.Interfaces;

namespace UserService.Database.Repositories.Implementation;

internal class UserRepository(UserDbContext context) : IUserRepository
{
    public async Task<User?> Get(Guid id)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task Create(User user)
    {
        user.Password = HashPassword(user.Password);
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task<User?> GetByUsername(string username)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> UsernameExists(string username)
    {
        return await context.Users.AnyAsync(u => u.Username == username);
    }

    private string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes

        // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
        var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return $"{Convert.ToBase64String(salt)}|{hashed}";
    }
}