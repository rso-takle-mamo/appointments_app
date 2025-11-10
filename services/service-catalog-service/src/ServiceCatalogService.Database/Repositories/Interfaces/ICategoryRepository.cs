using ServiceCatalogService.Database.Entities;
using ServiceCatalogService.Database.Models;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Database.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<(IEnumerable<Category> Categories, int TotalCount)> GetCategoriesAsync(PaginationParameters parameters, Guid? tenantId = null);
    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task CreateCategoryAsync(Category category);
    Task<bool> UpdateCategoryAsync(Guid id, UpdateCategory updateRequest);
    Task<bool> DeleteCategoryAsync(Guid id);
}