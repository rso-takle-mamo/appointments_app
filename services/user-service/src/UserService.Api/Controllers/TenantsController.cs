using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Requests;
using UserService.Api.Filters;
using UserService.Api.Exceptions;
using UserService.Api.Services.Interfaces;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/tenants")]
public class TenantsController(ITenantService tenantService)
    : BaseApiController
{

    /// <summary>
    /// Get tenant information by ID
    /// </summary>
    /// <param name="id">The tenant ID to retrieve</param>
    /// <returns>Tenant information</returns>
    [HttpGet("{id}")]
    [Authorize]
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

    /// <summary>
    /// Update tenant information
    /// </summary>
    /// <param name="id">The tenant ID to update</param>
    /// <param name="request">Tenant update information</param>
    /// <returns>Updated tenant information</returns>
    [HttpPatch("{id}")]
    [Authorize]
    [ServiceFilter(typeof(ModelValidationFilter))]
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
}