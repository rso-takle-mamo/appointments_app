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

    public async Task<bool> Exists(Guid id)
    {
        return await context.Tenants
            .AnyAsync(t => t.Id == id);
    }
}