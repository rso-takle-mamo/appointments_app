using Microsoft.EntityFrameworkCore;
using ServiceCatalogService.Database.Entities;
using ServiceCatalogService.Database.Repositories.Interfaces;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Database.Repositories.Implementation;

public class ServiceRepository(ServiceCatalogDbContext context) : IServiceRepository
{
    public async Task<IReadOnlyCollection<Service>> GetAllServicesAsync(Guid? tenantId = null)
    {
        var query = context.Services
            .Include(s => s.Category)
            .AsNoTracking()
            .AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(s => s.TenantId == tenantId.Value);
        }

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync();
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

    public async Task<bool> ToggleServiceAsync(Guid id)
    {
        var service = await context.Services.FindAsync(id);
        if (service == null)
        {
            return false;
        }

        service.IsActive = !service.IsActive;
        service.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }
}