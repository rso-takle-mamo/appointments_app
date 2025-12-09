using System.Security.Claims;
using BookingService.Api.Services.Interfaces;
using BookingService.Api.Exceptions;

namespace BookingService.Api.Services;

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

    public Guid GetUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value
                         ?? httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new AuthenticationException("JWT", "Invalid or missing user_id claim in token");
        }
        return userId;
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
            throw new AuthorizationException("Booking", "write", "Access denied. Customers cannot modify provider resources.");
        }
    }

    public void ValidateCustomerAccess()
    {
        if (!IsCustomer())
        {
            throw new AuthorizationException("Booking", "write", "Access denied. Providers cannot create customer bookings.");
        }
    }
}