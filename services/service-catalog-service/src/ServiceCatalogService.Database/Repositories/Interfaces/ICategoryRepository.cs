using ServiceCatalogService.Database.Entities;
using ServiceCatalogService.Database.Models;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Database.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetCategoryByServiceIdAsync(Guid serviceId);
    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task<Category?> GetCategoryByNameAndTenantAsync(string name, Guid tenantId);
    Task<IReadOnlyCollection<Category>> GetCategoriesByTenantIdAsync(Guid tenantId);
    Task<(IReadOnlyCollection<Category>, int TotalCount)> GetCategoriesByTenantIdAsync(Guid tenantId, int offset, int limit);
    Task CreateCategoryAsync(Category category);
    Task<bool> UpdateCategoryAsync(Guid id, UpdateCategory updateRequest);
    Task<bool> DeleteCategoryAsync(Guid id);
}