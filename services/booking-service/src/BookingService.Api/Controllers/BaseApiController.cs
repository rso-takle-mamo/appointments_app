using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookingService.Api.Services.Interfaces;
using BookingService.Api.Exceptions;

namespace BookingService.Api.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected Guid GetUserIdFromToken()
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

    protected void ValidateTenantAccess(Guid requestedTenantId)
    {
        var userTenantId = GetTenantIdFromToken();
        if (userTenantId == null)
        {
            throw new AuthorizationException("Tenant", "access", "Users without a tenant cannot access tenant resources");
        }
        if (requestedTenantId != userTenantId.Value)
        {
            throw new AuthorizationException("Tenant", "access", "You are not authorized to access this tenant");
        }
    }
}