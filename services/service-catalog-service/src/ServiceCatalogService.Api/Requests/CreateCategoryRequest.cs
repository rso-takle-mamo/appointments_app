using System.ComponentModel.DataAnnotations;

namespace ServiceCatalogService.Api.Requests;

public class CreateCategoryRequest
{
    [Required(ErrorMessage = "Category name is required")]
    [MinLength(2, ErrorMessage = "Category name must be at least 2 characters")]
    [MaxLength(255, ErrorMessage = "Category name cannot exceed 255 characters")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [MinLength(1, ErrorMessage = "Description must not be empty")]
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}