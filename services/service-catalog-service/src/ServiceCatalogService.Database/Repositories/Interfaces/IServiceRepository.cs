using ServiceCatalogService.Database.Entities;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Database.Repositories.Interfaces;

public interface IServiceRepository
{
    Task<IReadOnlyCollection<Service>> GetAllServicesAsync(Guid? tenantId = null);
    Task<Service?> GetServiceByIdAsync(Guid id);
    Task CreateServiceAsync(Service service);
    Task<bool> UpdateServiceAsync(Guid id, UpdateService updateRequest);
    Task<bool> DeleteServiceAsync(Guid id);
    Task<bool> ToggleServiceAsync(Guid id);
}