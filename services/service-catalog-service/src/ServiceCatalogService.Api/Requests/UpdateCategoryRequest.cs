using System.ComponentModel.DataAnnotations;

namespace ServiceCatalogService.Api.Requests;

public class UpdateCategoryRequest
{
    [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-\.]+$", ErrorMessage = "Category name can only contain letters, numbers, spaces, hyphens, and periods")]
    public string? Name { get; set; }

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}