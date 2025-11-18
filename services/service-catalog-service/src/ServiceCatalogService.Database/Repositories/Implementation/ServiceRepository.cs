using Microsoft.EntityFrameworkCore;
using ServiceCatalogService.Database.Entities;
using ServiceCatalogService.Database.Models;
using ServiceCatalogService.Database.Repositories.Interfaces;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Database.Repositories.Implementation;

public class ServiceRepository(ServiceCatalogDbContext context) : IServiceRepository
{
    public async Task<(IReadOnlyCollection<Service> Services, int TotalCount)> GetServicesAsync(PaginationParameters parameters, ServiceFilterParameters filters)
    {
        var query = context.Services
            .Include(s => s.Category)
            .AsNoTracking()
            .AsQueryable();

        // Apply all filters
        if (filters.TenantId.HasValue)
        {
            query = query.Where(s => s.TenantId == filters.TenantId.Value);
        }

        if (filters.MinPrice.HasValue)
        {
            query = query.Where(s => s.Price >= filters.MinPrice.Value);
        }

        if (filters.MaxPrice.HasValue)
        {
            query = query.Where(s => s.Price <= filters.MaxPrice.Value);
        }

        if (filters.MaxDuration.HasValue)
        {
            query = query.Where(s => s.DurationMinutes <= filters.MaxDuration.Value);
        }

        if (filters.CategoryId.HasValue)
        {
            query = query.Where(s => s.CategoryId == filters.CategoryId.Value);
        }

        if (!string.IsNullOrEmpty(filters.CategoryName))
        {
            query = query.Where(s => s.Category != null && EF.Functions.ILike(s.Category.Name, $"%{filters.CategoryName}%"));
        }

        if (filters.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == filters.IsActive.Value);
        }

        var totalCount = await query.CountAsync();

        var services = await query
            .OrderBy(s => s.Name)
            .Skip(parameters.Offset)
            .Take(parameters.Limit)
            .ToListAsync();

        return (services, totalCount);
    }

    public async Task<Service?> GetServiceByIdAsync(Guid id)
    {
        return await context.Services
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    
    
    public async Task CreateServiceAsync(Service service)
    {
        service.Id = Guid.NewGuid();
        service.CreatedAt = DateTime.UtcNow;
        service.UpdatedAt = DateTime.UtcNow;
        context.Services.Add(service);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateServiceAsync(Guid id, UpdateService updateRequest)
    {
        var service = await GetServiceByIdAsync(id);
        if (service == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(updateRequest.Name))
        {
            service.Name = updateRequest.Name;
        }

        if (updateRequest.Description != null)
        {
            service.Description = updateRequest.Description;
        }

        if (updateRequest.Price.HasValue)
        {
            service.Price = updateRequest.Price.Value;
        }

        if (updateRequest.DurationMinutes.HasValue)
        {
            service.DurationMinutes = updateRequest.DurationMinutes.Value;
        }

        if (updateRequest.CategoryId.HasValue)
        {
            service.CategoryId = updateRequest.CategoryId.Value;
        }

        if (updateRequest.IsActive.HasValue)
        {
            service.IsActive = updateRequest.IsActive.Value;
        }

        service.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteServiceAsync(Guid id)
    {
        var service = await context.Services.FindAsync(id);
        if (service == null)
        {
            return false;
        }

        context.Services.Remove(service);
        await context.SaveChangesAsync();

        return true;
    }
}