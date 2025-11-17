using UserService.Api.Requests;
using UserService.Api.Responses;
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
            throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
        }

        // Authorization check: user can only access their own tenant
        if (tenant.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to access this tenant");
        }

        return tenant.ToResponse();
    }

    public async Task<TenantResponse> CreateTenantAsync(Guid userId, CreateTenantRequest request)
    {
        // Check if user already has a tenant
        var existingTenant = await tenantRepository.GetByOwnerId(userId);
        if (existingTenant != null)
        {
            throw new InvalidOperationException("User already has a tenant. Only one tenant per user is allowed.");
        }

        // Get user to verify they exist
        var user = await userRepository.Get(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Create new tenant
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            BusinessName = request.BusinessName,
            BusinessEmail = request.BusinessEmail,
            BusinessPhone = request.BusinessPhone,
            Address = request.Address,
            Description = request.Description
        };

        var createdTenant = await tenantRepository.CreateAsync(tenant);

        // Update user's TenantId and Role to Provider
        user.TenantId = createdTenant.Id;
        user.Role = UserRole.Provider;
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);

        return createdTenant.ToResponse();
    }

    public async Task<TenantResponse> UpdateTenantAsync(Guid tenantId, Guid userId, UpdateTenantRequest request)
    {
        var tenant = await tenantRepository.Get(tenantId);
        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
        }

        // Authorization check: user can only update their own tenant
        if (tenant.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this tenant");
        }

        // Only update fields that are provided and changed
        bool hasUpdates = false;

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

        // Only update if there are actual changes
        if (hasUpdates)
        {
            tenant.UpdatedAt = DateTime.UtcNow;
            var updatedTenant = await tenantRepository.UpdateAsync(tenant);
            if (updatedTenant == null)
            {
                throw new InvalidOperationException("Failed to update tenant");
            }
            return updatedTenant.ToResponse();
        }

        // No changes made, return existing tenant
        return tenant.ToResponse();
    }

    public async Task DeleteTenantAsync(Guid tenantId, Guid userId)
    {
        var tenant = await tenantRepository.Get(tenantId);
        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
        }

        // Authorization check: user can only delete their own tenant
        if (tenant.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this tenant");
        }

        // Get the user to remove tenant association and reset role to Customer
        var user = await userRepository.Get(userId);
        if (user != null)
        {
            user.TenantId = null;
            user.Role = UserRole.Customer;
            user.UpdatedAt = DateTime.UtcNow;
            await userRepository.UpdateAsync(user);
        }

        // Delete the tenant
        var deleted = await tenantRepository.DeleteAsync(tenantId);
        if (!deleted)
        {
            throw new InvalidOperationException("Failed to delete tenant");
        }
    }
}