using UserService.Database.Enums;

namespace UserService.Database.Entities;

public class User
{
    public Guid Id { get; set; }
    
    public required string Username { get; set; }
    
    public required string Password { get; set; }
    
    public required string FirstName { get; set; }
    
    public required string LastName { get; set; }
    
    public required string Email { get; set; }
    
    public UserRole Role { get; set; }
    
    public Guid? TenantId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}