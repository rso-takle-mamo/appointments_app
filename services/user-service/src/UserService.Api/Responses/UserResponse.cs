using System.Text.Json.Serialization;
using UserService.Database.Entities;
using UserService.Database.Enums;

namespace UserService.Api.Responses;

public class UserResponse
{
    /// <summary>
    /// The unique identifier for the user
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public required Guid Id { get; set; }

    /// <summary>
    /// The username of the user
    /// </summary>
    /// <example>john_doe</example>
    public required string Username { get; set; }

    /// <summary>
    /// The first name of the user
    /// </summary>
    /// <example>John</example>
    public required string FirstName { get; set; }

    /// <summary>
    /// The last name of the user
    /// </summary>
    /// <example>Doe</example>
    public required string LastName { get; set; }

    /// <summary>
    /// The email address of the user
    /// </summary>
    /// <example>john.doe@example.com</example>
    public required string Email { get; set; }

    /// <summary>
    /// The role of the user (Customer or Provider)
    /// </summary>
    /// <example>Customer</example>
    [JsonConverter(typeof(JsonStringEnumConverter<UserRole>))]
    public required UserRole Role { get; set; }

    /// <summary>
    /// The tenant ID if the user is a provider
    /// </summary>
    /// <example>null</example>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// The timestamp when the user was created
    /// </summary>
    /// <example>2025-12-09T10:00:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The timestamp when the user was last updated
    /// </summary>
    /// <example>2025-12-09T11:00:00Z</example>
    public DateTime UpdatedAt { get; set; }
}