using System.ComponentModel.DataAnnotations;

namespace AvailabilityService.Api.Models.Requests.BufferTime;

public class UpdateBufferTimeRequest
{
    [Range(0, 480, ErrorMessage = "Before minutes must be between 0 and 480")]
    public int? BeforeMinutes { get; set; }

    [Range(0, 480, ErrorMessage = "After minutes must be between 0 and 480")]
    public int? AfterMinutes { get; set; }

    /// <summary>
    /// Optional service category ID. If null, this makes it a global buffer time.
    /// If specified, makes it category-specific.
    /// </summary>
    public Guid? CategoryId { get; set; }
}