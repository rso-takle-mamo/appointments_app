using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using UserService.Model;

namespace UserService.Services;

public class UserRepository
{
    public async Task<User> Get(Guid id)
    {
        //TODO get from db
        var user = new User
        {
            Id = id,
            FirstName = Guid.NewGuid().ToString("N"),
            LastName = Guid.NewGuid().ToString("N"),
            Email = Guid.NewGuid().ToString("N"),
            UserType = UserType.Customer,
            Username = Guid.NewGuid().ToString("N"),
            Password = Guid.NewGuid().ToString("N"),
        };
        
        return user;
    }

    public async Task Create(User user)
    {
        //TODO insert into database
        user.Password = HashPassword(user.Password);
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