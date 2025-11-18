using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Api.Requests;
using UserService.Api.Responses;
using UserService.Api.Services;
using UserService.Api.Filters;
using UserService.Api.Exceptions;

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

        if (userTenantId == null)
        {
            throw new AuthorizationException("Tenant", "read", "Users without a tenant cannot access tenant resources");
        }

        if (id != userTenantId.Value)
        {
            throw new AuthorizationException("Tenant", "read", "You are not authorized to access this tenant");
        }

        var tenant = await tenantService.GetTenantAsync(id, userId);
        return Ok(tenant);
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

        if (userTenantId == null)
        {
            throw new AuthorizationException("Tenant", "update", "Users without a tenant cannot update tenant resources");
        }

        if (id != userTenantId.Value)
        {
            throw new AuthorizationException("Tenant", "update", "You are not authorized to update this tenant");
        }

        var tenant = await tenantService.UpdateTenantAsync(id, userId, request);
        return Ok(tenant);
    }
    
    
    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new AuthenticationException("token", "Invalid user token");
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