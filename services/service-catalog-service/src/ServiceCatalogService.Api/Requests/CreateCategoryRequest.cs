using System.ComponentModel.DataAnnotations;

namespace ServiceCatalogService.Api.Requests;

public class CreateCategoryRequest
{
    [Required(ErrorMessage = "Tenant ID is required")]
    public Guid TenantId { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [MinLength(2, ErrorMessage = "Category name must be at least 2 characters")]
    [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    public required string Name { get; set; }

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}