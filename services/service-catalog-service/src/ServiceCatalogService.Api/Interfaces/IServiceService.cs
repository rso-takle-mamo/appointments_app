using ServiceCatalogService.Api.Models.DTOs;
using ServiceCatalogService.Api.Models.Entities;

namespace ServiceCatalogService.Api.Interfaces;

public interface IServiceService
{
    Task<IEnumerable<Service>> GetAllServicesAsync(Guid? tenantId = null);
    Task<Service?> GetServiceByIdAsync(Guid id);
    Task<Service> CreateServiceAsync(CreateServiceRequest request);
    Task<bool> UpdateServiceAsync(Guid id, UpdateServiceRequest request);
    Task<bool> DeleteServiceAsync(Guid id);
    Task<bool> ToggleServiceAsync(Guid id);
}