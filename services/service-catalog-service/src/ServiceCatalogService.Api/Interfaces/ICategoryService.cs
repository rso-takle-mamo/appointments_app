using ServiceCatalogService.Api.Models.DTOs;
using ServiceCatalogService.Api.Models.Entities;

namespace ServiceCatalogService.Api.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task<Category> CreateCategoryAsync(CreateCategoryRequest request);
    Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(Guid id);
}