using Microsoft.AspNetCore.Mvc;
using AvailabilityService.Api.Models.Requests.TimeBlock;
using AvailabilityService.Api.Models.Responses;
using AvailabilityService.Api.Services;
using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Repositories.Interfaces;

namespace AvailabilityService.Api.Controllers;

[ApiController]
[Route("api/availability")]
public class TimeBlockController(
    ILogger<TimeBlockController> logger,
    ITimeBlockRepository timeBlockRepository,
    IUserContextService userContextService)
    : ControllerBase
{
    /// <summary>
    /// Get all time blocks
    /// </summary>
    /// <param name="tenantId">Optional tenant ID to filter by. If not provided, returns time blocks for current user's tenant.</param>
    /// <param name="startDate">Optional start date filter (ISO 8601 format)</param>
    /// <param name="endDate">Optional end date filter (ISO 8601 format)</param>
    /// <returns>A successful response with time blocks data</returns>
    [HttpGet("time-blocks")]
    public async Task<IActionResult> GetTimeBlocks([FromQuery] Guid? tenantId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            Guid targetTenantId;
            var userRole = userContextService.GetRole();

            if (userContextService.IsCustomer())
            {
                // Customer must provide tenantId in query
                if (!tenantId.HasValue)
                {
                    return BadRequest(new { Message = "tenantId query parameter is required for customers" });
                }
                targetTenantId = tenantId.Value;
                userContextService.ValidateTenantAccess(targetTenantId, "TimeBlocks");
            }
            else // Provider
            {
                // Provider should NOT provide tenantId in query, use their own
                if (tenantId.HasValue)
                {
                    return BadRequest(new { Message = "Providers should not provide tenantId parameter. They can only access their own time blocks." });
                }
                targetTenantId = userContextService.GetTenantId();
            }

            logger.LogInformation("Getting time blocks for tenant: {TenantId}, startDate: {StartDate}, endDate: {EndDate}, role: {Role}",
                targetTenantId, startDate, endDate, userRole);

            IEnumerable<TimeBlock> timeBlocks;

            if (startDate.HasValue && endDate.HasValue)
            {
                // Get time blocks within date range
                var (blocks, _) = await timeBlockRepository.GetTimeBlocksByDateRangeAsync(startDate.Value, endDate.Value, targetTenantId);
                timeBlocks = blocks;
            }
            else
            {
                // Get all time blocks for tenant
                timeBlocks = await timeBlockRepository.GetTimeBlocksByTenantAsync(targetTenantId);
            }

            var response = timeBlocks.Select(tb => new TimeBlockResponse
            {
                Id = tb.Id,
                TenantId = tb.TenantId,
                StartDateTime = tb.StartDateTime,
                EndDateTime = tb.EndDateTime,
                Type = tb.Type,
                Reason = tb.Reason,
                RecurrencePattern = tb.RecurrencePattern,
                IsRecurring = tb.IsRecurring,
                ExternalEventId = tb.ExternalEventId,
                CreatedAt = tb.CreatedAt,
                UpdatedAt = tb.UpdatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting time blocks");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get a specific time block by ID
    /// </summary>
    /// <param name="id">Time block ID</param>
    /// <returns>Time block details</returns>
    [HttpGet("time-blocks/{id}")]
    public async Task<IActionResult> GetTimeBlockById(Guid id)
    {
        try
        {
            logger.LogInformation("Getting time block: {Id}", id);

            var timeBlock = await timeBlockRepository.GetTimeBlockByIdAsync(id);
            if (timeBlock == null)
            {
                return NotFound(new { Message = "Time block not found" });
            }

            // Validate tenant access
            userContextService.ValidateTenantAccess(timeBlock.TenantId, "TimeBlock");

            var response = new TimeBlockResponse
            {
                Id = timeBlock.Id,
                TenantId = timeBlock.TenantId,
                StartDateTime = timeBlock.StartDateTime,
                EndDateTime = timeBlock.EndDateTime,
                Type = timeBlock.Type,
                Reason = timeBlock.Reason,
                RecurrencePattern = timeBlock.RecurrencePattern,
                IsRecurring = timeBlock.IsRecurring,
                ExternalEventId = timeBlock.ExternalEventId,
                CreatedAt = timeBlock.CreatedAt,
                UpdatedAt = timeBlock.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting time block: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new time block
    /// </summary>
    /// <param name="request">Time block creation request</param>
    /// <returns>Created time block</returns>
    [HttpPost("time-blocks")]
    public async Task<IActionResult> CreateTimeBlock([FromBody] CreateTimeBlockRequest request)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Creating time block for tenant: {TenantId}, type: {Type}", tenantId, request.Type);

            // Validate time range
            if (request.StartDateTime >= request.EndDateTime)
            {
                return BadRequest(new { Message = "Start time must be before end time" });
            }

            // Validate recurrence pattern if provided
            if (request.RecurrencePattern != null)
            {
                var validationErrors = ValidateRecurrencePattern(request.RecurrencePattern);
                if (validationErrors.Any())
                {
                    return BadRequest(new { Message = "Invalid recurrence pattern", Errors = validationErrors });
                }
            }

            var timeBlock = new TimeBlock
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                StartDateTime = request.StartDateTime,
                EndDateTime = request.EndDateTime,
                Type = request.Type,
                Reason = request.Reason,
                RecurrencePattern = request.RecurrencePattern,
                ExternalEventId = request.ExternalEventId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await timeBlockRepository.CreateTimeBlockAsync(timeBlock);

            var response = new TimeBlockResponse
            {
                Id = timeBlock.Id,
                TenantId = timeBlock.TenantId,
                StartDateTime = timeBlock.StartDateTime,
                EndDateTime = timeBlock.EndDateTime,
                Type = timeBlock.Type,
                Reason = timeBlock.Reason,
                RecurrencePattern = timeBlock.RecurrencePattern,
                IsRecurring = timeBlock.IsRecurring,
                ExternalEventId = timeBlock.ExternalEventId,
                CreatedAt = timeBlock.CreatedAt,
                UpdatedAt = timeBlock.UpdatedAt
            };

            return CreatedAtAction(nameof(GetTimeBlockById), new { id = timeBlock.Id }, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating time block");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update an existing time block
    /// </summary>
    /// <param name="id">Time block ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated time block</returns>
    [HttpPut("time-blocks/{id}")]
    public async Task<IActionResult> UpdateTimeBlock(Guid id, [FromBody] UpdateTimeBlockRequest request)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Updating time block: {Id} for tenant: {TenantId}", id, tenantId);

            // Validate time range
            if (request.StartDateTime >= request.EndDateTime)
            {
                return BadRequest(new { Message = "Start time must be before end time" });
            }

            // Validate recurrence pattern if provided
            if (request.RecurrencePattern != null)
            {
                var validationErrors = ValidateRecurrencePattern(request.RecurrencePattern);
                if (validationErrors.Any())
                {
                    return BadRequest(new { Message = "Invalid recurrence pattern", Errors = validationErrors });
                }
            }

            // Check if time block exists and belongs to this tenant
            var existingTimeBlock = await timeBlockRepository.GetTimeBlockByIdAsync(id);
            if (existingTimeBlock == null)
            {
                return NotFound(new { Message = "Time block not found" });
            }

            if (existingTimeBlock.TenantId != tenantId)
            {
                return Forbid();
            }

            var updateRequest = new Database.UpdateModels.UpdateTimeBlock
            {
                StartDateTime = request.StartDateTime,
                EndDateTime = request.EndDateTime,
                Type = request.Type,
                Reason = request.Reason,
                RecurrencePattern = request.RecurrencePattern,
                ExternalEventId = request.ExternalEventId
            };

            var success = await timeBlockRepository.UpdateTimeBlockAsync(id, updateRequest);
            if (!success)
            {
                return StatusCode(500, new { Message = "Failed to update time block" });
            }

            // Get updated time block
            var updatedTimeBlock = await timeBlockRepository.GetTimeBlockByIdAsync(id);
            if (updatedTimeBlock == null)
            {
                return StatusCode(500, new { Message = "Time block not found after update" });
            }

            var response = new TimeBlockResponse
            {
                Id = updatedTimeBlock.Id,
                TenantId = updatedTimeBlock.TenantId,
                StartDateTime = updatedTimeBlock.StartDateTime,
                EndDateTime = updatedTimeBlock.EndDateTime,
                Type = updatedTimeBlock.Type,
                Reason = updatedTimeBlock.Reason,
                RecurrencePattern = updatedTimeBlock.RecurrencePattern,
                IsRecurring = updatedTimeBlock.IsRecurring,
                ExternalEventId = updatedTimeBlock.ExternalEventId,
                CreatedAt = updatedTimeBlock.CreatedAt,
                UpdatedAt = updatedTimeBlock.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating time block: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete a time block
    /// </summary>
    /// <param name="id">Time block ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("time-blocks/{id}")]
    public async Task<IActionResult> DeleteTimeBlock(Guid id)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Deleting time block: {Id} for tenant: {TenantId}", id, tenantId);

            // Check if time block exists and belongs to this tenant
            var existingTimeBlock = await timeBlockRepository.GetTimeBlockByIdAsync(id);
            if (existingTimeBlock == null)
            {
                return NotFound(new { Message = "Time block not found" });
            }

            if (existingTimeBlock.TenantId != tenantId)
            {
                return Forbid();
            }

            var success = await timeBlockRepository.DeleteTimeBlockAsync(id);
            if (!success)
            {
                return StatusCode(500, new { Message = "Failed to delete time block" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting time block: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Bulk delete time blocks within a date range
    /// </summary>
    /// <param name="request">Bulk delete request with date range</param>
    /// <returns>Count of deleted time blocks</returns>
    [HttpDelete("time-blocks/bulk")]
    public async Task<IActionResult> BulkDeleteTimeBlocks([FromBody] BulkDeleteTimeBlocksRequest request)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Bulk deleting time blocks for tenant: {TenantId}, startDate: {StartDate}, endDate: {EndDate}",
                tenantId, request.StartDate, request.EndDate);

            // Validate date range
            if (request.StartDate >= request.EndDate)
            {
                return BadRequest(new { Message = "Start date must be before end date" });
            }

            // Validate reasonable date ranges
            if (request.EndDate < DateTime.UtcNow.Date)
            {
                return BadRequest(new { Message = "End date cannot be in the past" });
            }

            if (request.StartDate > DateTime.UtcNow.AddYears(1))
            {
                return BadRequest(new { Message = "Start date cannot be more than 1 year in the future" });
            }

            var deletedCount = await timeBlockRepository.DeleteTimeBlocksByDateRangeAsync(request.StartDate, request.EndDate, tenantId);

            logger.LogInformation("Successfully deleted {Count} time blocks for tenant: {TenantId}", deletedCount, tenantId);

            return Ok(new
            {
                Message = "Time blocks deleted successfully",
                DeletedCount = deletedCount
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk deleting time blocks for date range: {StartDate} to {EndDate}", request.StartDate, request.EndDate);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    private List<string> ValidateRecurrencePattern(RecurrencePattern pattern)
    {
        var errors = new List<string>();

        if (pattern.Frequency == RecurrenceFrequency.Weekly && (pattern.DaysOfWeek == null || !pattern.DaysOfWeek.Any()))
        {
            errors.Add("Days of week must be specified for weekly recurrence pattern");
        }

        if (pattern.Frequency == RecurrenceFrequency.Monthly && pattern.DayOfMonth == null)
        {
            errors.Add("Day of month must be specified for monthly recurrence pattern");
        }

        if (pattern.DayOfMonth.HasValue && (pattern.DayOfMonth < 1 || pattern.DayOfMonth > 31))
        {
            errors.Add("Day of month must be between 1 and 31");
        }

        if (pattern.Interval < 1)
        {
            errors.Add("Interval must be greater than 0");
        }

        if (pattern.EndDate.HasValue && pattern.EndDate.Value < DateTime.UtcNow.Date)
        {
            errors.Add("End date cannot be in the past");
        }

        if (pattern.MaxOccurrences.HasValue && pattern.MaxOccurrences.Value <= 0)
        {
            errors.Add("Max occurrences must be greater than 0");
        }

        return errors;
    }
}