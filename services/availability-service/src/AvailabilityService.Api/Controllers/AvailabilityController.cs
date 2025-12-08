using Microsoft.AspNetCore.Mvc;
using AvailabilityService.Api.Services.Interfaces;
using AvailabilityService.Api.Exceptions;
using AvailabilityService.Api.Models;

namespace AvailabilityService.Api.Controllers;

[Route("api/availability")]
public class AvailabilityController(
    ILogger<AvailabilityController> logger,
    IAvailabilityService availabilityService,
    IUserContextService userContextService)
    : BaseApiController
{
    [HttpGet("slots")]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] Guid? tenantId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
            // Validate required date parameters
            if (!startDate.HasValue || !endDate.HasValue)
            {
                var missingFields = new List<string>();
                if (!startDate.HasValue) missingFields.Add("startDate");
                if (!endDate.HasValue) missingFields.Add("endDate");

                throw new ValidationException($"Missing required parameters: {string.Join(", ", missingFields)}",
                    missingFields.Select(f => new ValidationError { Field = f, Message = $"{f} is required" }).ToList());
            }

            // Validate date range (max 1 month)
            if (endDate.Value - startDate.Value > TimeSpan.FromDays(31))
            {
                throw new ValidationException("Date range cannot exceed 1 month",
                    new List<ValidationError> {
                        new ValidationError { Field = "endDate", Message = "Date range cannot exceed 1 month" }
                    });
            }

            // Validate dates are not in the past
            if (startDate.Value < DateTime.UtcNow.Date)
            {
                throw new ValidationException("Start date cannot be in the past",
                    new List<ValidationError> {
                        new ValidationError { Field = "startDate", Message = "Start date cannot be in the past" }
                    });
            }

            Guid targetTenantId;

            if (userContextService.IsCustomer())
            {
                // Customer must provide tenantId in query
                if (!tenantId.HasValue)
                {
                    throw new ValidationException("tenantId query parameter is required for customers",
                        new List<ValidationError> {
                            new ValidationError { Field = "tenantId", Message = "tenantId query parameter is required for customers" }
                        });
                }
                targetTenantId = tenantId.Value;
                userContextService.ValidateTenantAccess(targetTenantId, "Availability");
            }
            else // Provider
            {
                // Provider should NOT provide tenantId in query, use their own
                if (tenantId.HasValue)
                {
                    throw new ValidationException("Providers should not provide tenantId parameter. They can only check their own availability.",
                        new List<ValidationError> {
                            new ValidationError { Field = "tenantId", Message = "Providers should not provide tenantId parameter. They can only check their own availability." }
                        });
                }
                targetTenantId = userContextService.GetTenantId();
            }

            logger.LogInformation("Getting available time ranges for tenant: {TenantId}, from: {StartDate} to: {EndDate}",
                targetTenantId, startDate, endDate);

            // Get available time ranges
            var response = await availabilityService.GetAvailableRangesAsync(
                targetTenantId,
                startDate!.Value,
                endDate!.Value);

            return Ok(response);
    }
}