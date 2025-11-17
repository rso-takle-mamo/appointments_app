using UserService.Database.Entities;

namespace UserService.Database.Repositories.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> Get(Guid id);
    Task<bool> Exists(Guid id);
}