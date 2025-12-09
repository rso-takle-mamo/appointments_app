using BookingService.Database.Entities;

namespace BookingService.Api.Requests;

public class GetBookingsRequest : PaginationRequest
{
    /// <example>2024-12-01T00:00:00Z</example>
    public DateTime? StartDate { get; set; }

    /// <example>2024-12-31T23:59:59Z</example>
    public DateTime? EndDate { get; set; }

    /// <example>Pending</example>
    public BookingStatus? Status { get; set; }
}