using System.ComponentModel.DataAnnotations;

namespace UserService.Api.Requests;

public class ProviderRegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Business name is required")]
    [StringLength(200, ErrorMessage = "Business name cannot be longer than 200 characters")]
    public string BusinessName { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "Invalid business email format")]
    [StringLength(100, ErrorMessage = "Business email cannot be longer than 100 characters")]
    public string? BusinessEmail { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
    public string? BusinessPhone { get; set; }
    
    [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
    public string? Address { get; set; }
    
    [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
    public string? Description { get; set; }
}