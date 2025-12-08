using System.ComponentModel.DataAnnotations;

namespace AvailabilityService.Api.Requests.WorkingHours;

public class CreateWeeklyScheduleRequest
{
    [Required(ErrorMessage = "Schedule is required")]
    [MinLength(1, ErrorMessage = "At least one schedule entry is required")]
    public List<WeeklyScheduleEntry> Schedule { get; set; } = new();
}

public class WeeklyScheduleEntry
{
    [Required(ErrorMessage = "Days are required")]
    [MinLength(1, ErrorMessage = "At least one day must be specified")]
    public List<DayOfWeek> Days { get; set; } = new();

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    /// <summary>
    /// If true, these days are marked as free days and no working hours will be created.
    /// If false or not provided, StartTime and EndTime must be specified.
    /// </summary>
    public bool IsWorkFree { get; set; } = false;

    [Range(1, 100, ErrorMessage = "Max concurrent bookings must be between 1 and 100")]
    public int? MaxConcurrentBookings { get; set; } = 1;
}