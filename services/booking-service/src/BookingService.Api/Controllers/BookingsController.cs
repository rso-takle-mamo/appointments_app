using Microsoft.AspNetCore.Mvc;
using BookingService.Api.Services;
using BookingService.Api.Services.Interfaces;
using BookingService.Api.Requests;
using BookingService.Api.Responses;

namespace BookingService.Api.Controllers;

[Route("api/[controller]")]
public class BookingsController(
    ILogger<BookingsController> logger,
    IBookingService bookingService,
    IUserContextService userContextService) : BaseApiController
{
    /// <summary>
    /// Create a new booking (Customers only)
    /// </summary>
    /// <remarks>
    /// Creates a new booking for the specified service and time.
    /// The end time is automatically calculated based on the service duration.
    ///
    /// Only customers can create bookings.
    /// </remarks>
    /// <param name="request">The booking creation request</param>
    /// <returns>The created booking details</returns>
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> CreateBooking([FromBody] CreateBookingRequest request)
    {
        // Only customers can create bookings
        userContextService.ValidateCustomerAccess();

        var userId = userContextService.GetUserId();
        var tenantId = userContextService.GetTenantId();

        logger.LogInformation("Creating booking for user {UserId}, service {ServiceId}, at {StartTime}",
            userId, request.ServiceId, request.StartDateTime);

        var booking = await bookingService.CreateBookingAsync(request, userId, tenantId);

        return CreatedAtAction(
            nameof(GetBookingById),
            new { id = booking.Id },
            booking);
    }

    /// <summary>
    /// Get a booking by ID
    /// </summary>
    /// <remarks>
    /// **CUSTOMERS:**
    /// Can only retrieve their own bookings
    ///
    /// **PROVIDERS:**
    /// Can retrieve any booking within their tenant
    /// </remarks>
    /// <param name="id">The booking ID</param>
    /// <returns>The booking details</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> GetBookingById(Guid id)
    {
        var userId = userContextService.GetUserId();
        var tenantId = userContextService.GetTenantId();
        var userRole = userContextService.GetRole();

        var booking = await bookingService.GetBookingByIdAsync(id, userId, tenantId, userRole);

        if (booking == null)
        {
            return NotFound();
        }

        return Ok(booking);
    }

    /// <summary>
    /// Get bookings with pagination and filtering
    /// </summary>
    /// <remarks>
    /// **CUSTOMERS:**
    /// Can only retrieve their own bookings
    ///
    /// **PROVIDERS:**
    /// Can retrieve any bookings within their tenant
    ///
    /// Filter options:
    /// - startDate: Filter bookings from this date onwards (UTC)
    /// - endDate: Filter bookings up to this date (UTC)
    /// - status: Filter by booking status (Pending, Confirmed, Completed, Cancelled)
    /// </remarks>
    /// <param name="request">The filter and pagination parameters</param>
    /// <returns>Paginated list of bookings</returns>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<BookingResponse>>> GetBookings([FromQuery] GetBookingsRequest request)
    {
        var userId = userContextService.GetUserId();
        var tenantId = userContextService.GetTenantId();
        var userRole = userContextService.GetRole();

        logger.LogInformation("Retrieving bookings for user {UserId}, role {Role}, with filters: {@Request}",
            userId, userRole, request);

        var (bookings, totalCount) = await bookingService.GetBookingsAsync(request, userId, tenantId, userRole);

        return Ok(bookings);
    }

    /// <summary>
    /// Cancel a booking (Customers only)
    /// </summary>
    /// <remarks>
    /// Cancels a booking and changes its status to Cancelled.
    /// Only customers can cancel their own bookings.
    ///
    /// Bookings can only be cancelled if they are not already cancelled or completed.
    /// </remarks>
    /// <param name="id">The booking ID to cancel</param>
    /// <returns>The updated booking details</returns>
    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<BookingResponse>> CancelBooking(Guid id)
    {
        // Only customers can cancel bookings
        userContextService.ValidateCustomerAccess();

        var userId = userContextService.GetUserId();
        var tenantId = userContextService.GetTenantId();

        logger.LogInformation("Cancelling booking {BookingId} for user {UserId}", id, userId);

        var booking = await bookingService.CancelBookingAsync(id, userId, tenantId);

        return Ok(booking);
    }
}