namespace ServiceCatalogService.Api.Responses;

public class ServiceResponse
{
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }
    
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid TenantId { get; set; }
    
    /// <example>Men basic cut</example>
    public string Name { get; set; } = null!;

    /// <example>Including wash, cut, and styling</example>
    public string? Description { get; set; }
    
    /// <example>50</example>
    public decimal Price { get; set; }

    /// <example>45</example>
    public int DurationMinutes { get; set; }

    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? CategoryId { get; set; }

    /// <example>Haircuts</example>
    public string? CategoryName { get; set; }
    
    /// <example>true</example>
    public bool IsActive { get; set; }
    
    /// <example>Company</example>
    public string? BusinessName { get; set; }

    /// <example>Ljubljana 1, 1000</example>
    public string? Address { get; set; }

    /// <example>2025-12-05T15:30:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <example>2025-12-05T15:30:01Z</example>
    public DateTime UpdatedAt { get; set; }
}