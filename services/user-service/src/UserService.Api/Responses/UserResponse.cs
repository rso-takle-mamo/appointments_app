using System.Text.Json.Serialization;
using UserService.Database.Entities;
using UserService.Database.Enums;

namespace UserService.Api.Responses;

public class UserResponse
{
    public required Guid Id { get; set; }

    public required string Username { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }
    
    public required string Email { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter<UserRole>))]
    public required UserRole Role { get; set; }

    public Guid? TenantId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}