using Microsoft.AspNetCore.Mvc;
using AvailabilityService.Api.Models.Requests.BufferTime;
using AvailabilityService.Api.Models.Responses;
using AvailabilityService.Api.Services;
using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Repositories.Interfaces;
using AvailabilityService.Database.UpdateModels;

namespace AvailabilityService.Api.Controllers;

[ApiController]
[Route("api/availability")]
public class BufferTimeController(
    ILogger<BufferTimeController> logger,
    IBufferTimeRepository bufferTimeRepository,
    IUserContextService userContextService)
    : ControllerBase
{
    /// <summary>
    /// Get all buffer times for the tenant
    /// </summary>
    /// <param name="tenantId">Optional tenant ID to filter by. If not provided, returns buffer times for current user's tenant.</param>
    /// <returns>A successful response with buffer times data</returns>
    [HttpGet("buffer-times")]
    public async Task<IActionResult> GetBufferTimes([FromQuery] Guid? tenantId = null)
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
                userContextService.ValidateTenantAccess(targetTenantId, "BufferTimes");
            }
            else // Provider
            {
                // Provider should NOT provide tenantId in query, use their own
                if (tenantId.HasValue)
                {
                    return BadRequest(new { Message = "Providers should not provide tenantId parameter. They can only access their own buffer times." });
                }
                targetTenantId = userContextService.GetTenantId();
            }

            logger.LogInformation("Getting buffer times for tenant: {TenantId}, role: {Role}", targetTenantId, userRole);

            var bufferTimes = await bufferTimeRepository.GetBufferTimesByTenantAsync(targetTenantId);

            var response = bufferTimes.Select(bt => new BufferTimeResponse
            {
                Id = bt.Id,
                TenantId = bt.TenantId,
                CategoryId = bt.CategoryId,
                BeforeMinutes = bt.BeforeMinutes,
                AfterMinutes = bt.AfterMinutes,
                CreatedAt = bt.CreatedAt,
                UpdatedAt = bt.UpdatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting buffer times");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get a specific buffer time by ID
    /// </summary>
    /// <param name="id">Buffer time ID</param>
    /// <returns>Buffer time details</returns>
    [HttpGet("buffer-times/{id}")]
    public async Task<IActionResult> GetBufferTimeById(Guid id)
    {
        try
        {
            logger.LogInformation("Getting buffer time: {Id}", id);

            var bufferTime = await bufferTimeRepository.GetBufferTimeByIdAsync(id);
            if (bufferTime == null)
            {
                return NotFound(new { Message = "Buffer time not found" });
            }

            // Validate tenant access
            userContextService.ValidateTenantAccess(bufferTime.TenantId, "BufferTime");

            var response = new BufferTimeResponse
            {
                Id = bufferTime.Id,
                TenantId = bufferTime.TenantId,
                CategoryId = bufferTime.CategoryId,
                BeforeMinutes = bufferTime.BeforeMinutes,
                AfterMinutes = bufferTime.AfterMinutes,
                CreatedAt = bufferTime.CreatedAt,
                UpdatedAt = bufferTime.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting buffer time: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new buffer time
    /// </summary>
    /// <param name="request">Buffer time creation request</param>
    /// <returns>Created buffer time</returns>
    [HttpPost("buffer-times")]
    public async Task<IActionResult> CreateBufferTime([FromBody] CreateBufferTimeRequest request)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Creating buffer time for tenant: {TenantId}, categoryId: {CategoryId}",
                tenantId, request.CategoryId);

            // Check if buffer time already exists for this tenant and category
            var existingBufferTime = await bufferTimeRepository.GetBufferTimeByTenantAndCategoryAsync(tenantId, request.CategoryId);
            if (existingBufferTime != null)
            {
                throw new Exceptions.AlreadyExistsException(
                    "BufferTime",
                    existingBufferTime.Id,
                    $"Buffer time already exists for this {(request.CategoryId.HasValue ? "category" : "global settings")}");
            }

            // Validate category exists if provided (would need cross-service validation)
            if (request.CategoryId.HasValue)
            {
                // TODO: Validate category exists in ServiceCatalog service
                // For now, we'll assume the category ID is valid
                logger.LogWarning("Category validation not implemented - accepting CategoryId: {CategoryId}", request.CategoryId.Value);
            }

            var bufferTime = new BufferTime
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CategoryId = request.CategoryId,
                BeforeMinutes = request.BeforeMinutes,
                AfterMinutes = request.AfterMinutes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await bufferTimeRepository.CreateBufferTimeAsync(bufferTime);

            var response = new BufferTimeResponse
            {
                Id = bufferTime.Id,
                TenantId = bufferTime.TenantId,
                CategoryId = bufferTime.CategoryId,
                BeforeMinutes = bufferTime.BeforeMinutes,
                AfterMinutes = bufferTime.AfterMinutes,
                CreatedAt = bufferTime.CreatedAt,
                UpdatedAt = bufferTime.UpdatedAt
            };

            return CreatedAtAction(nameof(GetBufferTimeById), new { id = bufferTime.Id }, response);
        }
        catch (Exceptions.AlreadyExistsException ex)
        {
            logger.LogWarning(ex, "Buffer time already exists");
            return Conflict(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                title = "Conflict",
                status = 409,
                detail = ex.Message,
                existingResourceId = ex.ExistingResourceId
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating buffer time");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update an existing buffer time
    /// </summary>
    /// <param name="id">Buffer time ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated buffer time</returns>
    [HttpPut("buffer-times/{id}")]
    public async Task<IActionResult> UpdateBufferTime(Guid id, [FromBody] UpdateBufferTimeRequest request)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Updating buffer time: {Id} for tenant: {TenantId}", id, tenantId);

            // Check if buffer time exists and belongs to this tenant
            var existingBufferTime = await bufferTimeRepository.GetBufferTimeByIdAsync(id);
            if (existingBufferTime == null)
            {
                return NotFound(new { Message = "Buffer time not found" });
            }

            if (existingBufferTime.TenantId != tenantId)
            {
                return Forbid();
            }

            // Check if updating to a category that already has a buffer time
            if (request.CategoryId != existingBufferTime.CategoryId)
            {
                var conflictBufferTime = await bufferTimeRepository.GetBufferTimeByTenantAndCategoryAsync(tenantId, request.CategoryId);
                if (conflictBufferTime != null && conflictBufferTime.Id != id)
                {
                    throw new Exceptions.AlreadyExistsException(
                        "BufferTime",
                        conflictBufferTime.Id,
                        $"Buffer time already exists for this {(request.CategoryId.HasValue ? "category" : "global settings")}");
                }
            }

            // Validate category exists if provided
            if (request.CategoryId.HasValue)
            {
                // TODO: Validate category exists in ServiceCatalog service
                logger.LogWarning("Category validation not implemented - accepting CategoryId: {CategoryId}", request.CategoryId.Value);
            }

            var updateRequest = new UpdateBufferTime
            {
                BeforeMinutes = request.BeforeMinutes,
                AfterMinutes = request.AfterMinutes
            };

            var success = await bufferTimeRepository.UpdateBufferTimeAsync(id, updateRequest);
            if (!success)
            {
                return StatusCode(500, new { Message = "Failed to update buffer time" });
            }

            // Get updated buffer time
            var updatedBufferTime = await bufferTimeRepository.GetBufferTimeByIdAsync(id);
            if (updatedBufferTime == null)
            {
                return StatusCode(500, new { Message = "Buffer time not found after update" });
            }

            var response = new BufferTimeResponse
            {
                Id = updatedBufferTime.Id,
                TenantId = updatedBufferTime.TenantId,
                CategoryId = updatedBufferTime.CategoryId,
                BeforeMinutes = updatedBufferTime.BeforeMinutes,
                AfterMinutes = updatedBufferTime.AfterMinutes,
                CreatedAt = updatedBufferTime.CreatedAt,
                UpdatedAt = updatedBufferTime.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exceptions.AlreadyExistsException ex)
        {
            logger.LogWarning(ex, "Buffer time already exists");
            return Conflict(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                title = "Conflict",
                status = 409,
                detail = ex.Message,
                existingResourceId = ex.ExistingResourceId
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating buffer time: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete a buffer time
    /// </summary>
    /// <param name="id">Buffer time ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("buffer-times/{id}")]
    public async Task<IActionResult> DeleteBufferTime(Guid id)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Deleting buffer time: {Id} for tenant: {TenantId}", id, tenantId);

            // Check if buffer time exists and belongs to this tenant
            var existingBufferTime = await bufferTimeRepository.GetBufferTimeByIdAsync(id);
            if (existingBufferTime == null)
            {
                return NotFound(new { Message = "Buffer time not found" });
            }

            if (existingBufferTime.TenantId != tenantId)
            {
                return Forbid();
            }

            var success = await bufferTimeRepository.DeleteBufferTimeAsync(id);
            if (!success)
            {
                return StatusCode(500, new { Message = "Failed to delete buffer time" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting buffer time: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }
}