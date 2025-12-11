using System.ComponentModel.DataAnnotations;

namespace UserService.Api.Requests;

public class ProviderRegisterRequest
{
    /// <summary>
    /// The username for the provider account
    /// </summary>
    /// <example>janes_business</example>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The password for the provider account
    /// </summary>
    /// <example>SecurePass123!</example>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// The first name of the provider
    /// </summary>
    /// <example>Jane</example>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// The last name of the provider
    /// </summary>
    /// <example>Smith</example>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// The email address of the provider
    /// </summary>
    /// <example>jane@business.com</example>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The VAT number of the provider
    /// </summary>
    /// <example>DE123456789</example>
    [Required(ErrorMessage = "VAT number is required")]
    [StringLength(20, ErrorMessage = "VAT number cannot be longer than 20 characters")]
    public string VatNumber { get; set; } = string.Empty;

    /// <summary>
    /// The business name of the provider
    /// </summary>
    /// <example>Jane's Professional Services</example>
    [Required(ErrorMessage = "Business name is required")]
    [StringLength(200, ErrorMessage = "Business name cannot be longer than 200 characters")]
    public string BusinessName { get; set; } = string.Empty;

    /// <summary>
    /// The business email address
    /// </summary>
    /// <example>contact@business.com</example>
    [EmailAddress(ErrorMessage = "Invalid business email format")]
    [StringLength(100, ErrorMessage = "Business email cannot be longer than 100 characters")]
    public string? BusinessEmail { get; set; }

    /// <summary>
    /// The business phone number
    /// </summary>
    /// <example>+1-555-123-4567</example>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
    public string? BusinessPhone { get; set; }

    /// <summary>
    /// The business address
    /// </summary>
    /// <example>123 Business St, City, State 12345</example>
    [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
    public string? Address { get; set; }

    /// <summary>
    /// The business description
    /// </summary>
    /// <example>Professional consulting and advisory services</example>
    [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
    public string? Description { get; set; }
}