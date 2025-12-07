using Microsoft.AspNetCore.Mvc;
using AvailabilityService.Api.Models.Requests.TenantSettings;
using AvailabilityService.Api.Models.Responses;
using AvailabilityService.Api.Services;
using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Repositories.Interfaces;
using AvailabilityService.Database.UpdateModels;

namespace AvailabilityService.Api.Controllers;

[ApiController]
[Route("api/availability")]
public class TenantSettingsController(
    ILogger<TenantSettingsController> logger,
    ITenantSettingsRepository tenantSettingsRepository,
    IUserContextService userContextService)
    : ControllerBase
{
    /// <summary>
    /// Get tenant settings
    /// </summary>
    /// <param name="tenantId">Optional tenant ID to filter by. If not provided, returns settings for current user's tenant.</param>
    /// <returns>Tenant settings</returns>
    [HttpGet("tenant-settings")]
    public async Task<IActionResult> GetTenantSettings([FromQuery] Guid? tenantId = null)
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
                userContextService.ValidateTenantAccess(targetTenantId, "TenantSettings");
            }
            else // Provider
            {
                // Provider should NOT provide tenantId in query, use their own
                if (tenantId.HasValue)
                {
                    return BadRequest(new { Message = "Providers should not provide tenantId parameter. They can only access their own tenant settings." });
                }
                targetTenantId = userContextService.GetTenantId();
            }

            logger.LogInformation("Getting tenant settings for tenant: {TenantId}, role: {Role}", targetTenantId, userRole);

            // Use the existing repository method that gets settings by tenant ID
            var tenantSettings = await tenantSettingsRepository.GetSettingsByTenantIdAsync(targetTenantId);
            if (tenantSettings == null)
            {
                return NotFound(new { Message = "Tenant settings not found" });
            }

            var response = new TenantSettingsResponse
            {
                Id = tenantSettings.Id,
                TenantId = tenantSettings.TenantId,
                TimeZone = tenantSettings.TimeZone,
                CreatedAt = tenantSettings.CreatedAt,
                UpdatedAt = tenantSettings.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant settings");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get tenant settings by ID
    /// </summary>
    /// <param name="id">Tenant settings ID</param>
    /// <returns>Tenant settings details</returns>
    [HttpGet("tenant-settings/{id}")]
    public async Task<IActionResult> GetTenantSettingsById(Guid id)
    {
        try
        {
            logger.LogInformation("Getting tenant settings: {Id}", id);

            var tenantSettings = await tenantSettingsRepository.GetSettingsByIdAsync(id);
            if (tenantSettings == null)
            {
                return NotFound(new { Message = "Tenant settings not found" });
            }

            // Validate tenant access
            userContextService.ValidateTenantAccess(tenantSettings.TenantId, "TenantSettings");

            var response = new TenantSettingsResponse
            {
                Id = tenantSettings.Id,
                TenantId = tenantSettings.TenantId,
                TimeZone = tenantSettings.TimeZone,
                CreatedAt = tenantSettings.CreatedAt,
                UpdatedAt = tenantSettings.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant settings: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create tenant settings
    /// </summary>
    /// <param name="request">Tenant settings creation request</param>
    /// <returns>Created tenant settings</returns>
    [HttpPost("tenant-settings")]
    public async Task<IActionResult> CreateTenantSettings([FromBody] CreateTenantSettingsRequest request)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Creating tenant settings for tenant: {TenantId}", tenantId);

            // Check if tenant settings already exist for this tenant
            var existingSettings = await tenantSettingsRepository.GetSettingsByTenantIdAsync(tenantId);
            if (existingSettings != null)
            {
                throw new Exceptions.AlreadyExistsException(
                    "TenantSettings",
                    existingSettings.Id,
                    "Tenant settings already exist for this tenant");
            }

            // Validate time zone
            if (!IsValidTimeZone(request.TimeZone))
            {
                return BadRequest(new { Message = "Invalid time zone format" });
            }

            var tenantSettings = new TenantSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TimeZone = request.TimeZone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await tenantSettingsRepository.CreateSettingsAsync(tenantSettings);

            var response = new TenantSettingsResponse
            {
                Id = tenantSettings.Id,
                TenantId = tenantSettings.TenantId,
                TimeZone = tenantSettings.TimeZone,
                CreatedAt = tenantSettings.CreatedAt,
                UpdatedAt = tenantSettings.UpdatedAt
            };

            return CreatedAtAction(nameof(GetTenantSettings), new { id = tenantSettings.Id }, response);
        }
        catch (Exceptions.AlreadyExistsException ex)
        {
            logger.LogWarning(ex, "Tenant settings already exist");
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
            logger.LogError(ex, "Error creating tenant settings");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update tenant settings
    /// </summary>
    /// <param name="id">Tenant settings ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated tenant settings</returns>
    [HttpPut("tenant-settings/{id}")]
    public async Task<IActionResult> UpdateTenantSettings(Guid id, [FromBody] UpdateTenantSettingsRequest request)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Updating tenant settings: {Id} for tenant: {TenantId}", id, tenantId);

            // Check if tenant settings exist and belong to this tenant
            var existingSettings = await tenantSettingsRepository.GetSettingsByIdAsync(id);
            if (existingSettings == null)
            {
                return NotFound(new { Message = "Tenant settings not found" });
            }

            if (existingSettings.TenantId != tenantId)
            {
                return Forbid();
            }

            // Validate time zone
            if (!IsValidTimeZone(request.TimeZone))
            {
                return BadRequest(new { Message = "Invalid time zone format" });
            }

            var updateRequest = new UpdateTenantSettings
            {
                TimeZone = request.TimeZone
            };

            var success = await tenantSettingsRepository.UpdateSettingsAsync(id, updateRequest);
            if (!success)
            {
                return StatusCode(500, new { Message = "Failed to update tenant settings" });
            }

            // Get updated tenant settings
            var updatedSettings = await tenantSettingsRepository.GetSettingsByIdAsync(id);
            if (updatedSettings == null)
            {
                return StatusCode(500, new { Message = "Tenant settings not found after update" });
            }

            var response = new TenantSettingsResponse
            {
                Id = updatedSettings.Id,
                TenantId = updatedSettings.TenantId,
                TimeZone = updatedSettings.TimeZone,
                CreatedAt = updatedSettings.CreatedAt,
                UpdatedAt = updatedSettings.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating tenant settings: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete tenant settings
    /// </summary>
    /// <param name="id">Tenant settings ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("tenant-settings/{id}")]
    public async Task<IActionResult> DeleteTenantSettings(Guid id)
    {
        try
        {
            // Validate provider access
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Deleting tenant settings: {Id} for tenant: {TenantId}", id, tenantId);

            // Check if tenant settings exist and belong to this tenant
            var existingSettings = await tenantSettingsRepository.GetSettingsByIdAsync(id);
            if (existingSettings == null)
            {
                return NotFound(new { Message = "Tenant settings not found" });
            }

            if (existingSettings.TenantId != tenantId)
            {
                return Forbid();
            }

            var success = await tenantSettingsRepository.DeleteSettingsAsync(id);
            if (!success)
            {
                return StatusCode(500, new { Message = "Failed to delete tenant settings" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting tenant settings: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    private bool IsValidTimeZone(string timeZone)
    {
        try
        {
            // Basic validation - try to get the time zone info
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return tz != null;
        }
        catch
        {
            // If it fails, it's not a valid time zone
            return false;
        }
    }
}