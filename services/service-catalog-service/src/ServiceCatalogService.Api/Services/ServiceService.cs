using ServiceCatalogService.Api.Requests;
using ServiceCatalogService.Api.Responses;
using ServiceCatalogService.Api.Extensions;
using ServiceCatalogService.Api.Exceptions;
using ServiceCatalogService.Api.Services.Interfaces;
using ServiceCatalogService.Database.Models;
using ServiceCatalogService.Database.Enums;
using ServiceCatalogService.Database.Repositories.Interfaces;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Api.Services;

public class ServiceService(IServiceRepository serviceRepository) : IServiceService
{
    public async Task<(IEnumerable<ServiceResponse> Services, int TotalCount)> GetServicesAsync(
        ServiceFilterRequest request,
        bool isCustomer,
        Guid? userTenantId)
    {
        if (!isCustomer)
        {
            if (!string.IsNullOrEmpty(request.Address) || !string.IsNullOrEmpty(request.BusinessName))
            {
                throw new AuthorizationException("Service", "filter", "Address and BusinessName filters are available only to customers.");
            }
        }

        var parameters = new PaginationParameters
        {
            Offset = request.Offset,
            Limit = request.Limit,
            Sort = new SortParameters
            {
                Field = request.OrderBy ?? ServiceSortField.Name,
                Direction = request.OrderDirection ?? SortDirection.Ascending
            }
        };

        ServiceFilterParameters filters;

        if (isCustomer)
        {
            filters = new ServiceFilterParameters
            {
                TenantId = request.TenantId,
                MinPrice = request.MinPrice,
                MaxPrice = request.MaxPrice,
                MaxDuration = request.MaxDuration,
                ServiceName = request.ServiceName,
                CategoryId = request.CategoryId,
                CategoryName = request.CategoryName,
                IsActive = request.IsActive,
                Address = request.Address,
                BusinessName = request.BusinessName
            };
        }
        else
        {
            // providers should not attempt to specify tenantId in query parameters
            if (request.TenantId.HasValue)
            {
                throw new AuthorizationException("Service", "filter", "Providers cannot specify tenantId parameter. Tenant access is automatically enforced from authentication token.");
            }

            filters = new ServiceFilterParameters
            {
                TenantId = userTenantId,
                MinPrice = request.MinPrice,
                MaxPrice = request.MaxPrice,
                MaxDuration = request.MaxDuration,
                ServiceName = request.ServiceName,
                CategoryId = request.CategoryId,
                CategoryName = request.CategoryName,
                IsActive = request.IsActive,
                Address = null,
                BusinessName = null
            };
        }

        var (services, totalCount) = await serviceRepository.GetServicesAsync(parameters, filters);
        var serviceResponses = services.Select(s => s.ToServiceResponse()).ToList();

        return (serviceResponses, totalCount);
    }

    public async Task<ServiceResponse?> GetServiceByIdAsync(Guid id, bool isCustomer, Guid? userTenantId)
    {
        var service = await serviceRepository.GetServiceByIdAsync(id);

        if (service == null)
        {
            throw new NotFoundException("Service", id);
        }

        if (isCustomer) return service.ToServiceResponse();
        
        return service.TenantId != userTenantId ? throw new AuthorizationException("Service", "access", "Access denied. Service belongs to a different tenant.") : service.ToServiceResponse();
    }

    public async Task<ServiceResponse> CreateServiceAsync(CreateServiceRequest request, Guid tenantId)
    {
        var service = request.ToEntity(tenantId);
        service.Id = Guid.NewGuid();

        await serviceRepository.CreateServiceAsync(service);

        return service.ToServiceResponse();
    }

    public async Task<ServiceResponse> UpdateServiceAsync(Guid id, UpdateServiceRequest request, Guid userTenantId)
    {
        var existingService = await serviceRepository.GetServiceByIdAsync(id);

        if (existingService == null)
        {
            throw new NotFoundException("Service", id);
        }

        if (existingService.TenantId != userTenantId)
        {
            throw new AuthorizationException("Service", "update", "Access denied. Cannot update service from different tenant.");
        }

        var updateRequest = new UpdateService
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
            CategoryId = request.CategoryId,
            IsActive = request.IsActive,
        };

        var success = await serviceRepository.UpdateServiceAsync(id, updateRequest);

        if (!success)
        {
            throw new DatabaseOperationException("Update", "Service", "Failed to update service");
        }

        var updatedService = await serviceRepository.GetServiceByIdAsync(id);
        return updatedService!.ToServiceResponse();
    }

    public async Task<bool> DeleteServiceAsync(Guid id, Guid userTenantId)
    {
        var existingService = await serviceRepository.GetServiceByIdAsync(id);

        if (existingService == null)
        {
            throw new NotFoundException("Service", id);
        }

        if (existingService.TenantId != userTenantId)
        {
            throw new AuthorizationException("Service", "delete", "Access denied. Cannot delete service from different tenant.");
        }

        var success = await serviceRepository.DeleteServiceAsync(id);

        return !success ? throw new DatabaseOperationException("Delete", "Service", "Failed to delete service") : true;
    }
}