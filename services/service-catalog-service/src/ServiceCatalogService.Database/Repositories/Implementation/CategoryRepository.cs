using Microsoft.EntityFrameworkCore;
using ServiceCatalogService.Database.Entities;
using ServiceCatalogService.Database.Repositories.Interfaces;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Database.Repositories.Implementation;

public class CategoryRepository(ServiceCatalogDbContext context) : ICategoryRepository
{
    public async Task<Category?> GetCategoryByServiceIdAsync(Guid serviceId)
    {
        return await context.Services
            .Where(s => s.Id == serviceId)
            .Select(s => s.Category)
            .FirstOrDefaultAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await context.Categories.FindAsync(id);
    }

    public async Task<Category?> GetCategoryByNameAndTenantAsync(string name, Guid tenantId)
    {
        return await context.Categories
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Name == name);
    }

  
    public async Task<IReadOnlyCollection<Category>> GetCategoriesByTenantIdAsync(Guid tenantId)
    {
        return await context.Categories
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<(IReadOnlyCollection<Category>, int TotalCount)> GetCategoriesByTenantIdAsync(Guid tenantId, int offset, int limit)
    {
        var query = context.Categories
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.Name)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var categories = await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return (categories, totalCount);
    }

    public async Task CreateCategoryAsync(Category category)
    {
        category.Id = Guid.NewGuid();
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;
        context.Categories.Add(category);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateCategoryAsync(Guid id, UpdateCategory updateRequest)
    {
        var category = await GetCategoryByIdAsync(id);
        if (category == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(updateRequest.Name))
        {
            category.Name = updateRequest.Name;
        }

        if (updateRequest.Description != null)
        {
            category.Description = updateRequest.Description;
        }

        category.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await GetCategoryByIdAsync(id);
        if (category == null)
        {
            return false;
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return true;
    }
}