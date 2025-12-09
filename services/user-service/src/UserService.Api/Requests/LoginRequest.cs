using System.ComponentModel.DataAnnotations;

namespace UserService.Api.Requests;

public class LoginRequest
{
    /// <summary>
    /// The username for login
    /// </summary>
    /// <example>john_doe</example>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The password for login
    /// </summary>
    /// <example>SecurePass123!</example>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;
}