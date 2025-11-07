using Microsoft.EntityFrameworkCore;
using ServiceCatalogService.Api.Data;
using ServiceCatalogService.Api.Extensions;
using ServiceCatalogService.Api.Interfaces;
using ServiceCatalogService.Api.Models.DTOs;
using ServiceCatalogService.Api.Models.Entities;

namespace ServiceCatalogService.Api.Services;

public class ServiceService(ApplicationDbContext context) : IServiceService
{
    public async Task<IEnumerable<Service>> GetAllServicesAsync(Guid? tenantId = null)
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

    public async Task<Service> CreateServiceAsync(CreateServiceRequest request)
    {
        var service = request.ToEntity();

        context.Services.Add(service);
        await context.SaveChangesAsync();

        return service;
    }

    public async Task<bool> UpdateServiceAsync(Guid id, UpdateServiceRequest request)
    {
        var service = await context.Services.FindAsync(id);
        if (service == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            service.Name = request.Name;
        }

        if (request.Description != null)
        {
            service.Description = request.Description;
        }

        if (request.Price.HasValue)
        {
            service.Price = request.Price.Value;
        }

        if (request.DurationMinutes.HasValue)
        {
            service.DurationMinutes = request.DurationMinutes.Value;
        }

        if (request.CategoryId.HasValue)
        {
            service.CategoryId = request.CategoryId.Value;
        }

        if (request.IsActive.HasValue)
        {
            service.IsActive = request.IsActive.Value;
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