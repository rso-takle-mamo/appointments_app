using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AvailabilityService.Api.Requests.TenantSettings;
using AvailabilityService.Api.Responses;
using AvailabilityService.Api.Services.Interfaces;

namespace AvailabilityService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/availability")]
public class TenantSettingsController(
    ILogger<TenantSettingsController> logger,
    ITenantSettingsService tenantSettingsService,
    IUserContextService userContextService)
    : BaseApiController
{
    /// <summary>
    /// Get buffer settings for the provider's tenant
    /// </summary>
    /// <remarks>
    /// **PROVIDERS ONLY**
    /// - Buffer time is added before and after appointments
    /// - Default values are 0 minutes
    /// </remarks>
    /// <returns>Buffer settings in minutes</returns>
    [HttpGet("tenant-settings/buffer")]
    public async Task<IActionResult> GetBufferSettings()
    {
        try
        {
            // Only providers can access their tenant settings
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Getting buffer settings for tenant: {TenantId}", tenantId);

            var (bufferBefore, bufferAfter) = await tenantSettingsService.GetBufferSettingsAsync(tenantId);

            var response = new BufferSettingsResponse
            {
                BufferBeforeMinutes = bufferBefore,
                BufferAfterMinutes = bufferAfter
            };

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, "Tenant not found");
            return NotFound(new { Message = ex.Message });
        }
    }


/// <summary>
    /// Update buffer settings for the provider's tenant
    /// </summary>
    /// <remarks>
    /// **PROVIDERS ONLY**
    /// - Buffer time is added before and after appointments
    /// - Used to prevent back-to-back bookings
    /// - Helps with preparation time between appointments
    /// </remarks>
    /// <param name="request">Buffer settings to update</param>
    /// <returns>Updated buffer settings</returns>
    [HttpPatch("tenant-settings/buffer")]
    public async Task<IActionResult> UpdateBufferSettings([FromBody] PatchBufferSettingsRequest request)
    {
        try
        {
            // Only providers can update their tenant settings
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Updating buffer settings for tenant: {TenantId}, Before: {Before}, After: {After}",
                tenantId, request.BufferBeforeMinutes, request.BufferAfterMinutes);

            var (bufferBefore, bufferAfter) = await tenantSettingsService.UpdateBufferSettingsAsync(
                tenantId,
                request.BufferBeforeMinutes,
                request.BufferAfterMinutes);

            var response = new BufferSettingsResponse
            {
                BufferBeforeMinutes = bufferBefore,
                BufferAfterMinutes = bufferAfter
            };

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, "Tenant not found");
            return NotFound(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid buffer settings: {Message}", ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
    }
      /// <summary>
    /// Reset buffer settings to default values (0 minutes)
    /// </summary>
    /// <remarks>
    /// **PROVIDERS ONLY**
    /// - Resets both bufferBefore and bufferAfter to 0
    /// - This removes any buffer time between appointments
    /// </remarks>
    /// <returns>No content on successful reset</returns>
    [HttpDelete("tenant-settings/buffer")]
    public async Task<IActionResult> ResetBufferSettings()
    {
        try
        {
            // Only providers can reset their buffer settings
            userContextService.ValidateProviderAccess();

            // Get tenant ID from JWT token
            var tenantId = userContextService.GetTenantId();

            logger.LogInformation("Resetting buffer settings for tenant: {TenantId}", tenantId);

            await tenantSettingsService.ResetBufferSettingsAsync(tenantId);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, "Tenant not found");
            return NotFound(new { Message = ex.Message });
        }
    }
}