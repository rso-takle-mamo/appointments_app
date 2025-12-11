using System.ComponentModel.DataAnnotations;

namespace UserService.Api.Requests;

public class UpdateTenantRequest
{
    /// <summary>
    /// The updated business email (optional)
    /// </summary>
    /// <example>updated@business.com</example>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Business email cannot exceed 255 characters")]
    public string? BusinessEmail { get; set; }

    /// <summary>
    /// The updated business phone (optional)
    /// </summary>
    /// <example>+1-555-987-6543</example>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(50, ErrorMessage = "Business phone cannot exceed 50 characters")]
    public string? BusinessPhone { get; set; }

    /// <summary>
    /// The updated business description (optional)
    /// </summary>
    /// <example>Updated business description</example>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}