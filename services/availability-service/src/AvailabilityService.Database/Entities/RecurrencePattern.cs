using System.Text.Json.Serialization;

namespace AvailabilityService.Database.Entities;

public class RecurrencePattern
{
    public RecurrenceFrequency Frequency { get; set; }
    public int Interval { get; set; } = 1; // Every 1 day, 2 weeks, etc.

    // For weekly patterns
    public DayOfWeek[]? DaysOfWeek { get; set; }

    // End condition (at least one should be specified for infinite recurrence)
    public DateTime? EndDate { get; set; }
    public int? MaxOccurrences { get; set; }

    /// <summary>
    /// Creates a daily recurrence pattern
    /// </summary>
    /// <param name="interval">Interval in days (default: 1)</param>
    /// <param name="endDate">Optional end date</param>
    /// <param name="maxOccurrences">Optional maximum number of occurrences</param>
    public static RecurrencePattern Daily(int interval = 1, DateTime? endDate = null, int? maxOccurrences = null)
        => new() { Frequency = RecurrenceFrequency.Daily, Interval = interval, EndDate = endDate, MaxOccurrences = maxOccurrences };

    /// <summary>
    /// Creates a weekly recurrence pattern
    /// </summary>
    /// <param name="daysOfWeek">Days of the week for recurrence</param>
    /// <param name="interval">Interval in weeks (default: 1)</param>
    /// <param name="endDate">Optional end date</param>
    /// <param name="maxOccurrences">Optional maximum number of occurrences</param>
    public static RecurrencePattern Weekly(DayOfWeek[] daysOfWeek, int interval = 1, DateTime? endDate = null, int? maxOccurrences = null)
        => new() { Frequency = RecurrenceFrequency.Weekly, Interval = interval, DaysOfWeek = daysOfWeek, EndDate = endDate, MaxOccurrences = maxOccurrences };

    /// <summary>
    /// Creates a weekdays-only recurrence pattern (Monday-Friday)
    /// </summary>
    /// <param name="interval">Interval in weeks (default: 1)</param>
    /// <param name="endDate">Optional end date</param>
    /// <param name="maxOccurrences">Optional maximum number of occurrences</param>
    public static RecurrencePattern Weekdays(int interval = 1, DateTime? endDate = null, int? maxOccurrences = null)
        => new() { Frequency = RecurrenceFrequency.Weekly, Interval = interval, DaysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }, EndDate = endDate, MaxOccurrences = maxOccurrences };

    /// <summary>
    /// Creates a weekends-only recurrence pattern (Saturday-Sunday)
    /// </summary>
    /// <param name="interval">Interval in weeks (default: 1)</param>
    /// <param name="endDate">Optional end date</param>
    /// <param name="maxOccurrences">Optional maximum number of occurrences</param>
    public static RecurrencePattern Weekends(int interval = 1, DateTime? endDate = null, int? maxOccurrences = null)
        => new() { Frequency = RecurrenceFrequency.Weekly, Interval = interval, DaysOfWeek = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday }, EndDate = endDate, MaxOccurrences = maxOccurrences };

    /// <summary>
    /// Creates a monthly recurrence pattern
    /// </summary>
    /// <param name="dayOfMonth">Day of the month (1-31)</param>
    /// <param name="interval">Interval in months (default: 1)</param>
    /// <param name="endDate">Optional end date</param>
    /// <param name="maxOccurrences">Optional maximum number of occurrences</param>
    public static RecurrencePattern Monthly(int dayOfMonth, int interval = 1, DateTime? endDate = null, int? maxOccurrences = null)
        => new() { Frequency = RecurrenceFrequency.Monthly, Interval = interval, DayOfMonth = dayOfMonth, EndDate = endDate, MaxOccurrences = maxOccurrences };

    /// <summary>
    /// Day of the month for monthly recurrence (1-31)
    /// </summary>
    [JsonPropertyName("dayOfMonth")]
    public int? DayOfMonth { get; set; }
}

public enum RecurrenceFrequency
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}