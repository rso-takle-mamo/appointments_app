namespace ServiceCatalogService.Api.Responses;

public class CategoryResponse
{
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }
    
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid TenantId { get; set; }
    
    /// <example>Haircuts</example>
    public string Name { get; set; } = null!;
    
    /// <example>Includes all different services that can be classified as haircuts for women or men</example>
    public string? Description { get; set; }
    
    /// <example>2025-12-05T15:30:00Z</example>
    public DateTime CreatedAt { get; set; }
    
    /// <example>2025-12-05T15:30:01Z</example>
    public DateTime UpdatedAt { get; set; }
}