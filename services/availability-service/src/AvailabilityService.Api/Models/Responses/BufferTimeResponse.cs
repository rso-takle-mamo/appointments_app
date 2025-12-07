using System.Text.Json.Serialization;

namespace AvailabilityService.Api.Models.Responses;

public class BufferTimeResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>
    /// Optional service category ID. Null means this is a global buffer time.
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Whether this is a global buffer time (true) or category-specific (false)
    /// </summary>
    public bool IsGlobal => !CategoryId.HasValue;

    public int BeforeMinutes { get; set; }
    public int AfterMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}