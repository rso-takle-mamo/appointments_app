using ServiceCatalogService.Api.Requests;
using ServiceCatalogService.Api.Responses;
using ServiceCatalogService.Database.Entities;

namespace ServiceCatalogService.Api.Extensions;

public static class MappingExtensions
{
    public static CategoryResponse ToCategoryResponse(this Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            TenantId = category.TenantId,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    public static ServiceResponse ToServiceResponse(this Service service)
    {
        return new ServiceResponse
        {
            Id = service.Id,
            TenantId = service.TenantId,
            Name = service.Name,
            Description = service.Description,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes,
            CategoryId = service.CategoryId,
            CategoryName = service.Category?.Name,
            IsActive = service.IsActive,
            CreatedAt = service.CreatedAt,
            UpdatedAt = service.UpdatedAt,
            BusinessName = service.Tenant?.BusinessName,
            Address = service.Tenant?.Address
        };
    }

    public static Category ToEntity(this CreateCategoryRequest request, Guid tenantId)
    {
        return new Category
        {
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description
        };
    }

    public static Service ToEntity(this CreateServiceRequest request, Guid tenantId)
    {
        return new Service
        {
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
            CategoryId = request.CategoryId,
            IsActive = request.IsActive
        };
    }
}