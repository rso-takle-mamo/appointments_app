using Microsoft.EntityFrameworkCore;
using ServiceCatalogService.Database.Entities;
using ServiceCatalogService.Database.Enums;
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
            .Include(s => s.Tenant)
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

        if (!string.IsNullOrEmpty(filters.ServiceName))
        {
            query = query.Where(s => EF.Functions.ILike(s.Name, $"%{filters.ServiceName}%"));
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

        // Tenant-based filtering
        if (!string.IsNullOrEmpty(filters.Address))
        {
            query = query.Where(s => s.Tenant != null && EF.Functions.ILike(s.Tenant.Address, $"%{filters.Address}%"));
        }

        if (!string.IsNullOrEmpty(filters.BusinessName))
        {
            query = query.Where(s => s.Tenant != null && EF.Functions.ILike(s.Tenant.BusinessName, $"%{filters.BusinessName}%"));
        }

        var totalCount = await query.CountAsync();

        // Apply dynamic ordering
        query = ApplySorting(query, parameters.Sort);

        var services = await query
            .Skip(parameters.Offset)
            .Take(parameters.Limit)
            .ToListAsync();

        return (services, totalCount);
    }

    public async Task<Service?> GetServiceByIdAsync(Guid id)
    {
        return await context.Services
            .Include(s => s.Category)
            .Include(s => s.Tenant)
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

    private static IQueryable<Service> ApplySorting(IQueryable<Service> query, SortParameters? sortParameters)
    {
        var sortField = sortParameters?.Field ?? ServiceSortField.Name;
        var sortDirection = sortParameters?.Direction ?? SortDirection.Ascending;

        query = sortField switch
        {
            ServiceSortField.Name => sortDirection == SortDirection.Ascending
                ? query.OrderBy(s => s.Name)
                : query.OrderByDescending(s => s.Name),

            ServiceSortField.Price => sortDirection == SortDirection.Ascending
                ? query.OrderBy(s => s.Price)
                : query.OrderByDescending(s => s.Price),

            ServiceSortField.Duration => sortDirection == SortDirection.Ascending
                ? query.OrderBy(s => s.DurationMinutes)
                : query.OrderByDescending(s => s.DurationMinutes),

            ServiceSortField.CreatedAt => sortDirection == SortDirection.Ascending
                ? query.OrderBy(s => s.CreatedAt)
                : query.OrderByDescending(s => s.CreatedAt),

            ServiceSortField.UpdatedAt => sortDirection == SortDirection.Ascending
                ? query.OrderBy(s => s.UpdatedAt)
                : query.OrderByDescending(s => s.UpdatedAt),

            _ => query.OrderBy(s => s.Name) // Default fallback
        };

        return query;
    }
}