using System.ComponentModel.DataAnnotations;

namespace ServiceCatalogService.Api.Requests;

public class CreateCategoryRequest
{
    /// <summary>
    /// The name of the category
    /// </summary>
    /// <example>Haircuts</example>
    [Required(ErrorMessage = "Category name is required")]
    [MinLength(2, ErrorMessage = "Category name must be at least 2 characters")]
    [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-\.]+$", ErrorMessage = "Category name can only contain letters, numbers, spaces, hyphens, and periods")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Description of the category
    /// </summary>
    /// <example>Includes all different services that can be classified as haircuts for women or men</example>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}