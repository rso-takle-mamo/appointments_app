using System.ComponentModel.DataAnnotations;

namespace ServiceCatalogService.Api.Requests;

public class UpdateServiceRequest
{
    /// <summary>
    /// The name of the service
    /// </summary>
    /// <example>Men basic cut</example>
    [MaxLength(255, ErrorMessage = "Service name cannot exceed 255 characters")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-\.]+$", ErrorMessage = "Service name can only contain letters, numbers, spaces, hyphens, and periods")]
    public string? Name { get; set; }
    
    /// <summary>
    /// Detailed description of the service
    /// </summary>
    /// <example>Including wash, cut, and styling</example>
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// The price of the service in your currency
    /// </summary>
    /// <example>50</example>
    [Range(0, 9999999999.99, ErrorMessage = "Price must be between 0 and 9,999,999,999.99")]
    public decimal? Price { get; set; }
    
    /// <summary>
    /// Duration of the service in minutes (max 8 hours)
    /// </summary>
    /// <example>45</example>
    [Range(1, 480, ErrorMessage = "Duration must be between 1 and 480 minutes")]
    public int? DurationMinutes { get; set; }
    
    /// <summary>
    /// Optional category ID to group related services
    /// </summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid? CategoryId { get; set; }
    
    /// <summary>
    /// Whether this service is currently available for booking. Set to true by default
    /// </summary>
    /// <example>true</example>
    public bool? IsActive { get; set; }
}