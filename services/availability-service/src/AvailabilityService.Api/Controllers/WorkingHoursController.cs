using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AvailabilityService.Api.Models.Requests;
using AvailabilityService.Api.Models.Responses;
using AvailabilityService.Api.Services;
using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Repositories.Interfaces;
using AvailabilityService.Database.UpdateModels;

namespace AvailabilityService.Api.Controllers;

[ApiController]
[Route("api/availability")]
public class WorkingHoursController(
    ILogger<WorkingHoursController> logger,
    IWorkingHoursRepository workingHoursRepository,
    IUserContextService userContextService)
    : ControllerBase
{
    /// <summary>
    /// Get all working hours
    /// </summary>
    /// <param name="tenantId">Optional tenant ID to filter by. If not provided, returns working hours for current user's tenant.</param>
    /// <param name="day">Optional day filter (Monday-Sunday)</param>
    /// <returns>A successful response with working hours data</returns>
    [HttpGet("working-hours")]
    public async Task<IActionResult> GetWorkingHours([FromQuery] Guid? tenantId = null, [FromQuery] DayOfWeek? day = null)
    {
        try
        {
            Guid targetTenantId;
            if (userContextService.IsCustomer())
            {
                // Customer must provide tenantId in query
                if (!tenantId.HasValue)
                {
                    return BadRequest(new { Message = "tenantId query parameter is required for customers" });
                }
                targetTenantId = tenantId.Value;
            }
            else
            {
                // Provider should NOT provide tenantId in query, use their own
                if (tenantId.HasValue)
                {
                    return BadRequest(new { Message = "Providers should not provide tenantId parameter. They can only access their own working hours." });
                }
                targetTenantId = userContextService.GetTenantId();
            }
            
            IEnumerable<WorkingHours> workingHours;

            if (day.HasValue)
            {
                var dayWorkingHours = await workingHoursRepository.GetWorkingHoursByTenantAndDayAsync(targetTenantId, day.Value);
                workingHours = dayWorkingHours != null ? new[] { dayWorkingHours } : Enumerable.Empty<WorkingHours>();
            }
            else
            {
                workingHours = await workingHoursRepository.GetWorkingHoursByTenantAsync(targetTenantId);
            }

            var response = workingHours.Select(wh => new WorkingHoursResponse
            {
                Id = wh.Id,
                TenantId = wh.TenantId,
                ServiceId = wh.ServiceId,
                Day = wh.Day,
                StartTime = wh.StartTime,
                EndTime = wh.EndTime,
                MaxConcurrentBookings = wh.MaxConcurrentBookings,
                CreatedAt = wh.CreatedAt,
                UpdatedAt = wh.UpdatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting working hours");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create working hours for a single day
    /// </summary>
    /// <param name="request">Working hours creation request</param>
    /// <returns>Created working hours</returns>
    [HttpPost("working-hours")]
    public async Task<IActionResult> CreateWorkingHours([FromBody] CreateWorkingHoursRequest request)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Creating working hours for tenant: {TenantId}, day: {Day}", tenantId, request.Day);

            // Validate time range
            if (request.StartTime >= request.EndTime)
            {
                return BadRequest(new { Message = "Start time must be before end time" });
            }

            // Check if working hours already exist for this day and tenant
            var existingWorkingHours = await workingHoursRepository.GetWorkingHoursByTenantAndDayAsync(tenantId, request.Day);
            if (existingWorkingHours != null)
            {
                return Conflict(new { Message = $"Working hours already exist for {request.Day}. Use PUT endpoint to update." });
            }

            var workingHours = new WorkingHours
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Day = request.Day,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                MaxConcurrentBookings = request.MaxConcurrentBookings ?? 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await workingHoursRepository.CreateWorkingHoursAsync(workingHours);

            var response = new WorkingHoursResponse
            {
                Id = workingHours.Id,
                TenantId = workingHours.TenantId,
                ServiceId = workingHours.ServiceId,
                Day = workingHours.Day,
                StartTime = workingHours.StartTime,
                EndTime = workingHours.EndTime,
                MaxConcurrentBookings = workingHours.MaxConcurrentBookings,
                CreatedAt = workingHours.CreatedAt,
                UpdatedAt = workingHours.UpdatedAt
            };

            return CreatedAtAction(nameof(GetWorkingHours), new { tenantId = workingHours.TenantId, day = workingHours.Day }, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating working hours");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create working hours for multiple days in one request
    /// </summary>
    /// <param name="request">Weekly schedule creation request</param>
    /// <returns>Created working hours summary</returns>
    [HttpPost("working-hours/batch")]
    public async Task<IActionResult> CreateWeeklySchedule([FromBody] CreateWeeklyScheduleRequest request)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Creating weekly schedule for tenant: {TenantId}", tenantId);

            // Validate request
            var validationErrors = new List<string>();

            foreach (var entry in request.Schedule)
            {
                if (!entry.IsWorkFree && (!entry.StartTime.HasValue || !entry.EndTime.HasValue))
                {
                    validationErrors.Add($"Start time and end time are required for work days in entry for days: {string.Join(", ", entry.Days)}");
                }

                if (entry.StartTime.HasValue && entry.EndTime.HasValue && entry.StartTime >= entry.EndTime)
                {
                    validationErrors.Add($"Start time must be before end time for days: {string.Join(", ", entry.Days)}");
                }
            }

            if (validationErrors.Any())
            {
                return BadRequest(new { Message = "Validation failed", Errors = validationErrors });
            }

            // Delete all existing working hours for this tenant
            await workingHoursRepository.DeleteWorkingHoursByTenantAsync(tenantId);

            // Create new working hours for non-free days
            var workingHoursToCreate = new List<WorkingHours>();
            var createdDays = new List<DayOfWeek>();

            foreach (var entry in request.Schedule)
            {
                if (!entry.IsWorkFree)
                {
                    foreach (var day in entry.Days)
                    {
                        var workingHours = new WorkingHours
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantId,
                            Day = day,
                            StartTime = entry.StartTime!.Value,
                            EndTime = entry.EndTime!.Value,
                            MaxConcurrentBookings = entry.MaxConcurrentBookings ?? 1,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        workingHoursToCreate.Add(workingHours);
                        createdDays.Add(day);
                    }
                }
            }

            var createdCount = await workingHoursRepository.CreateMultipleWorkingHoursAsync(workingHoursToCreate);

            return Ok(new
            {
                Message = "Weekly schedule created successfully",
                CreatedCount = createdCount,
                CreatedDays = createdDays.Select(d => d.ToString()).ToList(),
                FreeDays = Enum.GetValues<DayOfWeek>()
                    .Except(createdDays)
                    .Select(d => d.ToString())
                    .ToList()
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating weekly schedule");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update working hours by ID
    /// </summary>
    /// <param name="id">Working hours ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated working hours</returns>
    [HttpPut("working-hours/{id}")]
    public async Task<IActionResult> UpdateWorkingHours(Guid id, [FromBody] UpdateWorkingHoursRequest request)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Updating working hours: {Id} for tenant: {TenantId}", id, tenantId);

            // Validate time range
            if (request.StartTime >= request.EndTime)
            {
                return BadRequest(new { Message = "Start time must be before end time" });
            }

            // Check if working hours exists and belongs to this tenant
            var existingWorkingHours = await workingHoursRepository.GetWorkingHoursByIdAsync(id);
            if (existingWorkingHours == null)
            {
                return NotFound(new { Message = "Working hours not found" });
            }

            if (existingWorkingHours.TenantId != tenantId)
            {
                return Forbid();
            }

            var updateRequest = new UpdateWorkingHours
            {
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                MaxConcurrentBookings = request.MaxConcurrentBookings
            };

            var success = await workingHoursRepository.UpdateWorkingHoursAsync(id, updateRequest);
            if (!success)
            {
                return StatusCode(500, new { Message = "Failed to update working hours" });
            }

            // Get updated working hours
            var updatedWorkingHours = await workingHoursRepository.GetWorkingHoursByIdAsync(id);
            if (updatedWorkingHours == null)
            {
                return StatusCode(500, new { Message = "Working hours not found after update" });
            }

            var response = new WorkingHoursResponse
            {
                Id = updatedWorkingHours.Id,
                TenantId = updatedWorkingHours.TenantId,
                ServiceId = updatedWorkingHours.ServiceId,
                Day = updatedWorkingHours.Day,
                StartTime = updatedWorkingHours.StartTime,
                EndTime = updatedWorkingHours.EndTime,
                MaxConcurrentBookings = updatedWorkingHours.MaxConcurrentBookings,
                CreatedAt = updatedWorkingHours.CreatedAt,
                UpdatedAt = updatedWorkingHours.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating working hours: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete working hours by ID
    /// </summary>
    /// <param name="id">Working hours ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("working-hours/{id}")]
    public async Task<IActionResult> DeleteWorkingHours(Guid id)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Deleting working hours: {Id} for tenant: {TenantId}", id, tenantId);

            // Check if working hours exists and belongs to this tenant
            var existingWorkingHours = await workingHoursRepository.GetWorkingHoursByIdAsync(id);
            if (existingWorkingHours == null)
            {
                return NotFound(new { Message = "Working hours not found" });
            }

            if (existingWorkingHours.TenantId != tenantId)
            {
                return Forbid();
            }

            var success = await workingHoursRepository.DeleteWorkingHoursAsync(id);
            if (!success)
            {
                return StatusCode(500, new { Message = "Failed to delete working hours" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting working hours: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }
}