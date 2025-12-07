using System.ComponentModel.DataAnnotations;

namespace AvailabilityService.Api.Models.Requests.BufferTime;

public class CreateBufferTimeRequest
{
    [Range(0, 480, ErrorMessage = "Before minutes must be between 0 and 480")]
    public int BeforeMinutes { get; set; } = 0;

    [Range(0, 480, ErrorMessage = "After minutes must be between 0 and 480")]
    public int AfterMinutes { get; set; } = 0;

    /// <summary>
    /// Optional service category ID. If null, this creates a global buffer time for all services.
    /// If specified, creates a category-specific buffer time.
    /// </summary>
    public Guid? CategoryId { get; set; }
}