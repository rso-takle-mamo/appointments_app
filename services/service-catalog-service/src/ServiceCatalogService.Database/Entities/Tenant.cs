namespace ServiceCatalogService.Database.Entities;

public class Tenant
{
    public Guid Id { get; set; }

    public required string BusinessName { get; set; }

    public required string Address { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}