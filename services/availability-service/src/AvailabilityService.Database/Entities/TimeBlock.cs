using System;

namespace AvailabilityService.Database.Entities;

public enum TimeBlockType
{
    Vacation,
    Break,
    Custom,
    GoogleCalendarEvent
}

public enum RecurrencePattern
{
    None,
    Daily,
    Weekly,
    Monthly
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
    public bool IsRecurring { get; set; } = false;
    public RecurrencePattern Pattern { get; set; } = RecurrencePattern.None;
    public DayOfWeek[]? RecurringDays { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }

    // External sync
    public string? ExternalEventId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}