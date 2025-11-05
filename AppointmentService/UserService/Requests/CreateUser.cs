using System.ComponentModel.DataAnnotations;

namespace UserService.Requests;

public class CreateUser
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string FirstName { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    [StringLength(100, MinimumLength = 1)]
    public required string Email { get; set; }
    
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public required string Username { get; set; }
    
    [Required]
    [StringLength(20, MinimumLength = 6)]
    public required string Password { get; set; }
}