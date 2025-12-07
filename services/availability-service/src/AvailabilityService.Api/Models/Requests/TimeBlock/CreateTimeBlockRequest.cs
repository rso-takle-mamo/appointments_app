using System.ComponentModel.DataAnnotations;
using AvailabilityService.Database.Entities;

namespace AvailabilityService.Api.Models.Requests.TimeBlock;

public class CreateTimeBlockRequest
{
    [Required(ErrorMessage = "Start date and time is required")]
    public DateTime StartDateTime { get; set; }

    [Required(ErrorMessage = "End date and time is required")]
    public DateTime EndDateTime { get; set; }

    [Required(ErrorMessage = "Type is required")]
    public TimeBlockType Type { get; set; }

    public string? Reason { get; set; }

    /// <summary>
    /// Optional recurrence pattern. If null, this is a one-time time block.
    /// </summary>
    public RecurrencePattern? RecurrencePattern { get; set; }

    /// <summary>
    /// Optional external event ID (e.g., Google Calendar event ID)
    /// </summary>
    public string? ExternalEventId { get; set; }
}