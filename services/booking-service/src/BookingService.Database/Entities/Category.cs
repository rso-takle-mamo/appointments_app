namespace BookingService.Database.Entities;

public class Category
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = null!;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
}