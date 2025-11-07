using System.ComponentModel.DataAnnotations;

namespace ServiceCatalogService.Api.Models.DTOs;

public class UpdateServiceRequest
{
    [MaxLength(255, ErrorMessage = "Service name cannot exceed 255 characters")]
    public string? Name { get; set; }

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Range(0.01, 99999.99, ErrorMessage = "Price must be between 0.01 and 99999.99")]
    public decimal? Price { get; set; }

    [Range(5, 480, ErrorMessage = "Duration must be between 5 and 480 minutes")]
    public int? DurationMinutes { get; set; }

    public Guid? CategoryId { get; set; }

    public bool? IsActive { get; set; }
}