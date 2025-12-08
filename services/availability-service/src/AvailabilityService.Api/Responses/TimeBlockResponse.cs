using AvailabilityService.Database.Entities;

namespace AvailabilityService.Api.Responses;

public class TimeBlockResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public TimeBlockType Type { get; set; }
    public string? Reason { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}