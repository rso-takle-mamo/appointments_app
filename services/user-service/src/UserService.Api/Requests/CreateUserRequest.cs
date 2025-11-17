using System.ComponentModel.DataAnnotations;
using UserService.Database.Entities;

namespace UserService.Api.Requests;

public class CreateUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string LastName { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string Username { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string Password { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100, MinimumLength = 3)]
    public required string Email { get; set; }

    public Guid? TenantId { get; set; }
}