using System.ComponentModel.DataAnnotations;

namespace ServiceCatalogService.Api.Requests;

public class CreateServiceRequest
{
    [Required(ErrorMessage = "Service name is required")]
    [MaxLength(255, ErrorMessage = "Service name cannot exceed 255 characters")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-\.]+$", ErrorMessage = "Service name can only contain letters, numbers, spaces, hyphens, and periods")]
    public string Name { get; set; } = null!;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0, 9999999999.99, ErrorMessage = "Price must be between 0 and 9,999,999,999.99")]
    public decimal Price { get; set; }

    [Range(1, 480, ErrorMessage = "Duration must be between 1 and 480 minutes")]
    public int DurationMinutes { get; set; } = 30;

    public Guid? CategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}