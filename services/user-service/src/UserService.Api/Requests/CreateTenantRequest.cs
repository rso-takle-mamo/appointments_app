using System.ComponentModel.DataAnnotations;

namespace UserService.Api.Requests;

public class CreateTenantRequest
{
    [Required(ErrorMessage = "Business name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Business name must be between 2 and 200 characters")]
    public required string BusinessName { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Business email cannot exceed 255 characters")]
    public string? BusinessEmail { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(50, ErrorMessage = "Business phone cannot exceed 50 characters")]
    public string? BusinessPhone { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}