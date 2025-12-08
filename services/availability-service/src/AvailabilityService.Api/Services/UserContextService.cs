using System.Security.Claims;
using AvailabilityService.Api.Exceptions;
using AvailabilityService.Api.Services.Interfaces;

namespace AvailabilityService.Api.Services;

public class UserContextService(IHttpContextAccessor httpContextAccessor) : IUserContextService
{
    public Guid GetTenantId()
    {
        var tenantIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value
                          ?? httpContextAccessor.HttpContext?.User?.FindFirst("tenantId")?.Value;

        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new AuthenticationException("JWT", "Invalid or missing tenant_id claim in token");
        }
        return tenantId;
    }

    public string GetRole()
    {
        var roleClaim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value
                      ?? httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value;

        return string.IsNullOrEmpty(roleClaim) ? throw new AuthenticationException("JWT", "Invalid or missing role claim in token") : roleClaim;
    }

    public bool IsCustomer()
    {
        var role = GetRole();
        return role.Equals("Customer", StringComparison.OrdinalIgnoreCase);
    }

    public void ValidateTenantAccess(Guid tenantId, string resource)
    {
        var userTenantId = GetTenantId();
        if (userTenantId != tenantId)
        {
            throw new AuthorizationException(resource, "read", $"You are not authorized to access resource: '{resource}'");
        }
    }

    public void ValidateProviderAccess()
    {
        if (IsCustomer())
        {
            throw new AuthorizationException("WorkingHours", "write", "Access denied. Customers cannot modify working hours.");
        }
    }
}