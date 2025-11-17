using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Api.Requests;
using UserService.Api.Responses;
using UserService.Api.Services;
using UserService.Api.Filters;

namespace UserService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tenants")]
public class TenantsController(ITenantService tenantService) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTenant(Guid id)
    {
        var userId = GetUserIdFromToken();
        var userTenantId = GetTenantIdFromToken();

        // Users with null tenant_id cannot access tenant resources
        if (userTenantId == null)
        {
            throw new UnauthorizedAccessException("Users without a tenant cannot access tenant resources");
        }

        // Users can only access their own tenant
        if (id != userTenantId.Value)
        {
            throw new UnauthorizedAccessException("You are not authorized to access this tenant");
        }

        var tenant = await tenantService.GetTenantAsync(id, userId);
        return Ok(tenant);
    }

    [HttpPost]
    [ServiceFilter(typeof(ModelValidationFilter))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
    {
        var userId = GetUserIdFromToken();
        var userTenantId = GetTenantIdFromToken();

        // Only users with null tenant_id can create tenants
        if (userTenantId != null)
        {
            throw new UnauthorizedAccessException("Users with an existing tenant cannot create additional tenants");
        }

        var tenant = await tenantService.CreateTenantAsync(userId, request);
        return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, tenant);
    }

    [HttpPatch("{id}")]
    [ServiceFilter(typeof(ModelValidationFilter))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantRequest request)
    {
        var userId = GetUserIdFromToken();
        var userTenantId = GetTenantIdFromToken();

        // Users with null tenant_id cannot update tenant resources
        if (userTenantId == null)
        {
            throw new UnauthorizedAccessException("Users without a tenant cannot update tenant resources");
        }

        // Users can only update their own tenant
        if (id != userTenantId.Value)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this tenant");
        }

        var tenant = await tenantService.UpdateTenantAsync(id, userId, request);
        return Ok(tenant);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTenant(Guid id)
    {
        var userId = GetUserIdFromToken();
        var userTenantId = GetTenantIdFromToken();

        // Users with null tenant_id cannot delete tenant resources
        if (userTenantId == null)
        {
            throw new UnauthorizedAccessException("Users without a tenant cannot delete tenant resources");
        }

        // Users can only delete their own tenant
        if (id != userTenantId.Value)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this tenant");
        }

        await tenantService.DeleteTenantAsync(id, userId);
        return NoContent();
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }

        return userId;
    }

    private Guid? GetTenantIdFromToken()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim))
        {
            return null;
        }

        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
    }
}