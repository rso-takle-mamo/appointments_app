using System;

namespace AvailabilityService.Database.Entities;

public class BufferTime
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? CategoryId { get; set; }
    public int BeforeMinutes { get; set; } = 0;
    public int AfterMinutes { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}