using System.Text.Json.Serialization;

namespace AvailabilityService.Database.Entities;

public enum TimeBlockType
{
    Vacation,
    Break,
    Custom,
    GoogleCalendarEvent
}

public class TimeBlock
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public TimeBlockType Type { get; set; }
    public string? Reason { get; set; }

    // Recurrence support
    public RecurrencePattern? RecurrencePattern { get; set; }

    // External sync
    public string? ExternalEventId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Helper property to check if this time block is recurring
    /// </summary>
    [JsonIgnore]
    public bool IsRecurring => RecurrencePattern != null;
}