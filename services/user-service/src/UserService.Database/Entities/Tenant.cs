namespace UserService.Database.Entities;

public class Tenant
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public required string BusinessName { get; set; }

    public string? BusinessEmail { get; set; }

    public string? BusinessPhone { get; set; }

    public string? Address { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}