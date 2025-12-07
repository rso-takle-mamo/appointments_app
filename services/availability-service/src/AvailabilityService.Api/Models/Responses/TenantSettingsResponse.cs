namespace AvailabilityService.Api.Models.Responses;

public class TenantSettingsResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TimeZone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}