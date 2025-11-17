using System;

namespace AvailabilityService.Database.Entities;

public class TenantSettings
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}