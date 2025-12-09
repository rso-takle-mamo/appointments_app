using ServiceCatalogService.Api.Requests;
using ServiceCatalogService.Api.Responses;
using ServiceCatalogService.Database.Models;

namespace ServiceCatalogService.Api.Services.Interfaces;

public interface IServiceService
{
    Task<(IEnumerable<ServiceResponse> Services, int TotalCount)> GetServicesAsync(
        ServiceFilterRequest request,
        bool isCustomer,
        Guid? userTenantId);
    Task<ServiceResponse?> GetServiceByIdAsync(Guid id, bool isCustomer, Guid? userTenantId);
    Task<ServiceResponse> CreateServiceAsync(CreateServiceRequest request, Guid tenantId);
    Task<ServiceResponse> UpdateServiceAsync(Guid id, UpdateServiceRequest request, Guid userTenantId);
    Task<bool> DeleteServiceAsync(Guid id, Guid userTenantId);
}