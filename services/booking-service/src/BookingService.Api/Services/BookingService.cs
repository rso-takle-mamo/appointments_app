using BookingService.Api.Services.Interfaces;
using BookingService.Api.Responses;
using BookingService.Api.Requests;
using BookingService.Database.Entities;
using BookingService.Database.Repositories.Interfaces;
using BookingService.Api.Exceptions;

namespace BookingService.Api.Services;

public class BookingService(
    IBookingRepository bookingRepository,
    IServiceRepository serviceRepository) : IBookingService
{
    public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request, Guid userId, Guid tenantId)
    {
        // Validate that the start time is in the future
        if (request.StartDateTime <= DateTime.UtcNow)
        {
            throw new ValidationException("Booking start time must be in the future");
        }

        // Get the service to validate it exists and belongs to the tenant
        var service = await serviceRepository.GetByIdAsync(request.ServiceId);
        if (service == null)
        {
            throw new NotFoundException("Service", request.ServiceId);
        }

        if (service.TenantId != tenantId)
        {
            throw new AuthorizationException("Service", "access", "You can only book services from your own tenant");
        }

        // Calculate end time based on service duration
        var endDateTime = request.StartDateTime.AddMinutes(service.DurationMinutes);

        // TODO: Call availability service to check if time slot is free
        // gRPC call to availability service here

        // Create the booking
        var booking = new Booking
        {
            TenantId = tenantId,
            OwnerId = userId,
            ServiceId = request.ServiceId,
            StartDateTime = request.StartDateTime,
            EndDateTime = endDateTime,
            BookingStatus = BookingStatus.Pending,
            Notes = request.Notes
        };

        var createdBooking = await bookingRepository.CreateAsync(booking);

        return MapToResponse(createdBooking);
    }

    public async Task<BookingResponse?> GetBookingByIdAsync(Guid id, Guid userId, Guid tenantId, string userRole)
    {
        var booking = await bookingRepository.GetByIdAsync(id);
        if (booking == null)
        {
            return null;
        }

        // Authorization check
        if (userRole.Equals("Customer", StringComparison.OrdinalIgnoreCase))
        {
            // Customers can only see their own bookings
            if (booking.OwnerId != userId)
            {
                throw new AuthorizationException("Booking", "read", "You can only view your own bookings");
            }
        }
        else
        {
            // Providers can only see bookings from their tenant
            if (booking.TenantId != tenantId)
            {
                throw new AuthorizationException("Booking", "read", "You can only view bookings from your tenant");
            }
        }

        return MapToResponse(booking);
    }

    public async Task<(PaginatedResponse<BookingResponse> Bookings, int TotalCount)> GetBookingsAsync(
        GetBookingsRequest request,
        Guid userId,
        Guid tenantId,
        string userRole)
    {
        List<Booking> bookings;
        int totalCount;

        if (userRole.Equals("Customer", StringComparison.OrdinalIgnoreCase))
        {
            // Customers can only see their own bookings
            bookings = await bookingRepository.GetByOwnerIdAsync(userId);
            totalCount = bookings.Count;

            // Apply filters
            bookings = ApplyFilters(bookings, request);
        }
        else
        {
            // Providers can see all bookings from their tenant
            bookings = await bookingRepository.GetByTenantIdAsync(tenantId);
            totalCount = bookings.Count;

            // Apply filters
            bookings = ApplyFilters(bookings, request);
        }

        // Sort by StartDateTime ascending
        bookings = bookings.OrderBy(b => b.StartDateTime).ToList();

        // Apply pagination
        var paginatedBookings = bookings
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToList();

        var response = new PaginatedResponse<BookingResponse>
        {
            Offset = request.Offset,
            Limit = request.Limit,
            TotalCount = totalCount,
            Data = paginatedBookings.Select(MapToResponse).ToList()
        };

        return (response, totalCount);
    }

    public async Task<BookingResponse> CancelBookingAsync(Guid id, Guid userId, Guid tenantId)
    {
        var booking = await bookingRepository.GetByIdAsync(id);
        if (booking == null)
        {
            throw new NotFoundException("Booking", id);
        }

        // Authorization check - ensure booking belongs to the user
        if (booking.OwnerId != userId)
        {
            throw new AuthorizationException("Booking", "cancel", "You can only cancel your own bookings");
        }

        // Validate booking can be cancelled
        if (booking.BookingStatus == BookingStatus.Cancelled)
        {
            throw new ConflictException("Status", "Booking is already cancelled");
        }

        if (booking.BookingStatus == BookingStatus.Completed)
        {
            throw new ConflictException("Status", "Cannot cancel a completed booking");
        }

        // Update status
        booking.BookingStatus = BookingStatus.Cancelled;
        var updatedBooking = await bookingRepository.UpdateAsync(booking);

        return MapToResponse(updatedBooking);
    }

    private static List<Booking> ApplyFilters(List<Booking> bookings, GetBookingsRequest request)
    {
        if (request.StartDate.HasValue)
        {
            bookings = bookings.Where(b => b.StartDateTime >= request.StartDate.Value).ToList();
        }

        if (request.EndDate.HasValue)
        {
            bookings = bookings.Where(b => b.EndDateTime <= request.EndDate.Value).ToList();
        }

        if (request.Status.HasValue)
        {
            bookings = bookings.Where(b => b.BookingStatus == request.Status.Value).ToList();
        }

        return bookings;
    }

    private static BookingResponse MapToResponse(Booking booking)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            TenantId = booking.TenantId,
            OwnerId = booking.OwnerId,
            ServiceId = booking.ServiceId,
            StartDateTime = booking.StartDateTime,
            EndDateTime = booking.EndDateTime,
            Status = booking.BookingStatus,
            Notes = booking.Notes,
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt
        };
    }
}