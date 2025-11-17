using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Models;
using AvailabilityService.Database.UpdateModels;

namespace AvailabilityService.Database.Repositories.Interfaces;

public interface ITenantSettingsRepository
{
    Task<(IEnumerable<TenantSettings> Settings, int TotalCount)> GetSettingsAsync(PaginationParameters parameters, Guid? tenantId = null);
    Task<TenantSettings?> GetSettingsByIdAsync(Guid id);
    Task<TenantSettings?> GetSettingsByTenantIdAsync(Guid tenantId);
    Task CreateSettingsAsync(TenantSettings settings);
    Task<bool> UpdateSettingsAsync(Guid id, UpdateTenantSettings updateRequest);
    Task<bool> DeleteSettingsAsync(Guid id);
}