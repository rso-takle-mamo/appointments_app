namespace AvailabilityService.Api.Models.Responses;

public class WorkingHoursResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? ServiceId { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int MaxConcurrentBookings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}