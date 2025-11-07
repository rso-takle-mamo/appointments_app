using Microsoft.EntityFrameworkCore;
using ServiceCatalogService.Api.Data;
using ServiceCatalogService.Api.Extensions;
using ServiceCatalogService.Api.Interfaces;
using ServiceCatalogService.Api.Models.DTOs;
using ServiceCatalogService.Api.Models.Entities;

namespace ServiceCatalogService.Api.Services;

public class CategoryService(ApplicationDbContext context) : ICategoryService
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

    public async Task<Category> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = request.ToEntity();

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return category;
    }

    public async Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
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