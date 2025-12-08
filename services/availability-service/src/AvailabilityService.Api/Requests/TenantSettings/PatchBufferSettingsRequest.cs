using System.ComponentModel.DataAnnotations;

namespace AvailabilityService.Api.Requests.TenantSettings;

public class PatchBufferSettingsRequest
{
    [Range(0, 480, ErrorMessage = "Buffer before minutes must be between 0 and 480")]
    public int? BufferBeforeMinutes { get; set; }
    
    [Range(0, 480, ErrorMessage = "Buffer after minutes must be between 0 and 480")]
    public int? BufferAfterMinutes { get; set; }
}