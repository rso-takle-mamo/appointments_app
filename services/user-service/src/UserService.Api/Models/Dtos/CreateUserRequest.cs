using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using UserService.Api.Models.Entities;

namespace UserService.Api.Models.Dtos;

public class CreateUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    [JsonPropertyName("firstName")]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    [JsonPropertyName("lastName")]
    public required string LastName { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    [JsonPropertyName("username")]
    public required string Username { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [JsonPropertyName("password")]
    public required string Password { get; set; }

    [StringLength(20, MinimumLength = 1)]
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("tenantId")]
    public Guid? TenantId { get; set; }
}