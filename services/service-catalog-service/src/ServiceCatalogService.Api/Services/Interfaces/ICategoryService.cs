using ServiceCatalogService.Api.Requests;
using ServiceCatalogService.Api.Responses;

namespace ServiceCatalogService.Api.Services.Interfaces;

public interface ICategoryService
{
    Task<CategoryResponse?> GetServiceCategoryAsync(Guid serviceId, bool isCustomer, Guid? userTenantId);
    Task<IEnumerable<CategoryResponse>> GetCategoriesAsync(Guid? tenantId, bool isCustomer, Guid? userTenantId);
    Task<(IEnumerable<CategoryResponse>, int TotalCount)> GetCategoriesAsync(Guid? tenantId, bool isCustomer, Guid? userTenantId, int offset, int limit);
    Task<CategoryResponse?> GetCategoryByIdAsync(Guid categoryId, Guid userTenantId);
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, Guid tenantId);
    Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request, Guid userTenantId);
    Task<bool> DeleteCategoryAsync(Guid categoryId, Guid userTenantId);
}