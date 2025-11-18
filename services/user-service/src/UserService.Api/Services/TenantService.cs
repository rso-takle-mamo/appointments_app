using UserService.Api.Requests;
using UserService.Api.Responses;
using UserService.Api.Exceptions;
using UserService.Database.Entities;
using UserService.Database.Repositories.Interfaces;
using UserService.Database.Enums;

namespace UserService.Api.Services;

public class TenantService(
    ITenantRepository tenantRepository,
    IUserRepository userRepository
) : ITenantService
{
    public async Task<TenantResponse> GetTenantAsync(Guid tenantId, Guid userId)
    {
        var tenant = await tenantRepository.Get(tenantId);
        if (tenant == null)
        {
            throw new NotFoundException("Tenant", tenantId);
        }

        return tenant.OwnerId != userId ? throw new AuthorizationException("Tenant", "read", "You are not authorized to access this tenant") : tenant.ToResponse();
    }

    
    public async Task<TenantResponse> UpdateTenantAsync(Guid tenantId, Guid userId, UpdateTenantRequest request)
    {
        var tenant = await tenantRepository.Get(tenantId);
        if (tenant == null)
        {
            throw new NotFoundException("Tenant", tenantId);
        }

        if (tenant.OwnerId != userId)
        {
            throw new AuthorizationException("Tenant", "update", "You are not authorized to update this tenant");
        }

        var hasUpdates = false;
        if (!string.IsNullOrEmpty(request.BusinessName) && request.BusinessName != tenant.BusinessName)
        {
            tenant.BusinessName = request.BusinessName;
            hasUpdates = true;
        }

        if (request.BusinessEmail != tenant.BusinessEmail)
        {
            tenant.BusinessEmail = request.BusinessEmail;
            hasUpdates = true;
        }

        if (request.BusinessPhone != tenant.BusinessPhone)
        {
            tenant.BusinessPhone = request.BusinessPhone;
            hasUpdates = true;
        }

        if (request.Address != tenant.Address)
        {
            tenant.Address = request.Address;
            hasUpdates = true;
        }

        if (request.Description != tenant.Description)
        {
            tenant.Description = request.Description;
            hasUpdates = true;
        }

        if (!hasUpdates) return tenant.ToResponse();
        
        tenant.UpdatedAt = DateTime.UtcNow;
        var updatedTenant = await tenantRepository.UpdateAsync(tenant);
        return updatedTenant == null ? throw new DatabaseOperationException("update", "Tenant", "Failed to update tenant") : updatedTenant.ToResponse();
    }
}