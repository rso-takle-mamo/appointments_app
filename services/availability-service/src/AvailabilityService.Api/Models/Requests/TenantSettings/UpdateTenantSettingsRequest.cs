using System.ComponentModel.DataAnnotations;

namespace AvailabilityService.Api.Models.Requests.TenantSettings;

public class UpdateTenantSettingsRequest
{
    [Required(ErrorMessage = "Time zone is required")]
    [StringLength(50, ErrorMessage = "Time zone cannot exceed 50 characters")]
    public string TimeZone { get; set; } = "UTC";
}