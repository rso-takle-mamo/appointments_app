using Microsoft.EntityFrameworkCore;
using UserService.Database.Entities;

namespace UserService.Database.Repositories.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> Get(Guid id);
    Task<Tenant> CreateAsync(Tenant tenant);
    Task<Tenant?> UpdateAsync(Tenant tenant);
    Task<bool> DeleteAsync(Guid id);
}