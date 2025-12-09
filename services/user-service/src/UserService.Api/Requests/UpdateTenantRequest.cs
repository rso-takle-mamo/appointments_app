using System.ComponentModel.DataAnnotations;

namespace UserService.Api.Requests;

public class UpdateTenantRequest
{
    /// <summary>
    /// The updated business name (optional)
    /// </summary>
    /// <example>Updated Business Name</example>
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Business name must be between 2 and 200 characters")]
    public string? BusinessName { get; set; }

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
    /// The updated business address (optional)
    /// </summary>
    /// <example>456 New Address, City, State 67890</example>
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    /// <summary>
    /// The updated business description (optional)
    /// </summary>
    /// <example>Updated business description</example>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}