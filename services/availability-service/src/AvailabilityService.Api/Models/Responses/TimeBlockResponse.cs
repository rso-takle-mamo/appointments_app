using System.Text.Json.Serialization;
using AvailabilityService.Database.Entities;

namespace AvailabilityService.Api.Models.Responses;

public class TimeBlockResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public TimeBlockType Type { get; set; }
    public string? Reason { get; set; }

    /// <summary>
    /// Recurrence pattern for this time block (null if one-time)
    /// </summary>
    public RecurrencePattern? RecurrencePattern { get; set; }

    /// <summary>
    /// Whether this time block is recurring
    /// </summary>
    public bool IsRecurring { get; set; }

    public string? ExternalEventId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}