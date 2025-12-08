using AvailabilityService.Api.Responses;

namespace AvailabilityService.Api.Services.Interfaces;

public interface IAvailabilityService
{
    Task<AvailableTimeRangeResponse> GetAvailableRangesAsync(Guid tenantId, DateTime startDate, DateTime endDate);
}