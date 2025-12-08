using System.ComponentModel.DataAnnotations;

namespace AvailabilityService.Api.Requests.TimeBlock;

public class BulkDeleteTimeBlocksRequest
{
    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }
}