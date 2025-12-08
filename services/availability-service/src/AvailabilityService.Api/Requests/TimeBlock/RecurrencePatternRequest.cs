using System.ComponentModel.DataAnnotations;
using AvailabilityService.Api.Attributes;

namespace AvailabilityService.Api.Requests.TimeBlock;

public class RecurrencePatternRequest
{
    [Required(ErrorMessage = "Frequency is required")]
    [StringEnum("Daily", "Weekly", "Monthly", ErrorMessage = "Frequency must be one of: Daily, Weekly, Monthly")]
    public string Frequency { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Interval must be a positive number")]
    public int? Interval { get; set; } = 1;

    // For weekly patterns - days of week (0=Sunday, 6=Saturday)
    public int[]? DaysOfWeek { get; set; }

    // For monthly patterns - days of month (1-31, or -1 for last day, -2 for second to last, etc.)
    public int[]? DaysOfMonth { get; set; }

    // End condition - one of these is required but not both
    public DateTime? EndDate { get; set; }
    public int? MaxOccurrences { get; set; }
}