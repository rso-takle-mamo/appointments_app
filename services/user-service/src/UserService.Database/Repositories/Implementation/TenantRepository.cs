using Microsoft.EntityFrameworkCore;
using UserService.Database.Entities;
using UserService.Database.Repositories.Interfaces;

namespace UserService.Database.Repositories.Implementation;

internal class TenantRepository(UserDbContext context) : ITenantRepository
{
    public async Task<Tenant?> Get(Guid id)
    {
        return await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    // Unused methods removed:
    // public async Task<bool> Exists(Guid id)
    // public async Task<Tenant?> GetByOwnerId(Guid ownerId)

    public async Task<Tenant> CreateAsync(Tenant tenant)
    {
        tenant.CreatedAt = DateTime.UtcNow;
        tenant.UpdatedAt = DateTime.UtcNow;

        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        return tenant;
    }

    public async Task<Tenant?> UpdateAsync(Tenant tenant)
    {
        tenant.UpdatedAt = DateTime.UtcNow;

        context.Tenants.Update(tenant);
        await context.SaveChangesAsync();

        return tenant;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tenant == null)
            return false;

        context.Tenants.Remove(tenant);
        await context.SaveChangesAsync();

        return true;
    }
}