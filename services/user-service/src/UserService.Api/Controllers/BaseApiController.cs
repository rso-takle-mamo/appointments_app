using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Exceptions;
using UserService.Database.Enums;

namespace UserService.Api.Controllers;

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
    protected Guid? GetTenantIdFromToken()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim))
        {
            return null;
        }
        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
    }
    
    protected UserRole GetUserRoleFromToken()
    {
        var roleClaim = User.FindFirst("role")?.Value;
        if (string.IsNullOrEmpty(roleClaim) || !int.TryParse(roleClaim, out var roleInt))
        {
            throw new AuthenticationException("token", "Invalid role claim in token");
        }
        return (UserRole)roleInt;
    }
    
    protected void ValidateTenantAccess(Guid requestedTenantId)
    {
        var userTenantId = GetTenantIdFromToken();
        if (userTenantId == null)
        {
            throw new AuthorizationException(
                "Tenant",
                "access",
                "Users without a tenant cannot access tenant resources");
        }
        if (requestedTenantId != userTenantId.Value)
        {
            throw new AuthorizationException(
                "Tenant",
                "access",
                "You are not authorized to access this tenant");
        }
    }
}