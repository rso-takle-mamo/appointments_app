using System.Runtime.Serialization;

namespace UserService.Model;

public class User
{
    public required Guid Id { get; set; }
    
    public required string FirstName { get; set; }
    
    public required string LastName { get; set; }
    
    public required string Email { get; set; }
    
    public required UserType UserType { get; set; }

    public required string Username { get; set; }

    public required string Password { get; set; }
}