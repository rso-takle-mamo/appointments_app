using System.ComponentModel.DataAnnotations;

namespace UserService.Api.Requests;

public class UpdateUserRequest
{
    /// <summary>
    /// The updated username (optional)
    /// </summary>
    /// <example>john_doe_updated</example>
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string? Username { get; set; }

    /// <summary>
    /// The updated first name (optional)
    /// </summary>
    /// <example>John</example>
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string? FirstName { get; set; }

    /// <summary>
    /// The updated last name (optional)
    /// </summary>
    /// <example>Smith</example>
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string? LastName { get; set; }

    /// <summary>
    /// The updated email address (optional)
    /// </summary>
    /// <example>john.smith@example.com</example>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string? Email { get; set; }
}