using Microsoft.EntityFrameworkCore;
using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Models;
using AvailabilityService.Database.Repositories.Interfaces;
using AvailabilityService.Database.UpdateModels;

namespace AvailabilityService.Database.Repositories.Implementation;

public class TenantSettingsRepository(AvailabilityDbContext context) : ITenantSettingsRepository
{
    public async Task<(IEnumerable<TenantSettings> Settings, int TotalCount)> GetSettingsAsync(PaginationParameters parameters, Guid? tenantId = null)
    {
        var query = context.TenantSettings
            .AsNoTracking()
            .AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(ts => ts.TenantId == tenantId.Value);
        }

        var totalCount = await query.CountAsync();

        var settings = await query
            .OrderBy(ts => ts.CreatedAt)
            .Skip(parameters.Offset)
            .Take(parameters.Limit)
            .ToListAsync();

        return (settings, totalCount);
    }

    public async Task<TenantSettings?> GetSettingsByIdAsync(Guid id)
    {
        return await context.TenantSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(ts => ts.Id == id);
    }

    public async Task<TenantSettings?> GetSettingsByTenantIdAsync(Guid tenantId)
    {
        return await context.TenantSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(ts => ts.TenantId == tenantId);
    }

    public async Task CreateSettingsAsync(TenantSettings settings)
    {
        settings.Id = Guid.NewGuid();
        settings.CreatedAt = DateTime.UtcNow;
        settings.UpdatedAt = DateTime.UtcNow;

        await context.TenantSettings.AddAsync(settings);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateSettingsAsync(Guid id, UpdateTenantSettings updateRequest)
    {
        var existingSettings = await context.TenantSettings.FindAsync(id);
        if (existingSettings == null) return false;

        if (updateRequest.TimeZone != null)
            existingSettings.TimeZone = updateRequest.TimeZone;

        existingSettings.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteSettingsAsync(Guid id)
    {
        var settings = await context.TenantSettings.FindAsync(id);
        if (settings == null) return false;

        context.TenantSettings.Remove(settings);
        await context.SaveChangesAsync();
        return true;
    }
}