using Microsoft.EntityFrameworkCore;
using ServiceCatalogService.Database.Entities;
using ServiceCatalogService.Database.Repositories.Interfaces;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Database.Repositories.Implementation;

public class CategoryRepository(ServiceCatalogDbContext context) : ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await context.Categories.FindAsync(id);
    }

    public async Task CreateCategoryAsync(Category category)
    {
        category.Id = Guid.NewGuid();
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;
        context.Categories.Add(category);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateCategoryAsync(Guid id, UpdateCategory request)
    {
        var category = await context.Categories.FindAsync(id);
        if (category == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            category.Name = request.Name;
        }

        if (request.Description != null)
        {
            category.Description = request.Description;
        }
        
        category.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category == null)
        {
            return false;
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync();

        return true;
    }
}