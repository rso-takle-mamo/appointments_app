using System.ComponentModel.DataAnnotations;


namespace ServiceCatalogService.Api.Models.DTOs;

public class UpdateCategoryRequest
{
    [MinLength(2, ErrorMessage = "Category name must be at least 2 characters")]
    [MaxLength(255, ErrorMessage = "Category name cannot exceed 255 characters")]
    public string? Name { get; set; }

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}