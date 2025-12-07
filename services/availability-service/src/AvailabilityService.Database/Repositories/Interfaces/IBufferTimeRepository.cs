using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Models;
using AvailabilityService.Database.UpdateModels;

namespace AvailabilityService.Database.Repositories.Interfaces;

public interface IBufferTimeRepository
{
    Task<(IEnumerable<BufferTime> BufferTimes, int TotalCount)> GetBufferTimesAsync(PaginationParameters parameters, Guid? tenantId = null);
    Task<BufferTime?> GetBufferTimeByIdAsync(Guid id);
    Task<BufferTime?> GetBufferTimeByTenantIdAsync(Guid tenantId);
    Task<BufferTime?> GetBufferTimeByTenantAndCategoryAsync(Guid tenantId, Guid? categoryId);
    Task<IEnumerable<BufferTime>> GetBufferTimesByTenantAsync(Guid tenantId);
    Task CreateBufferTimeAsync(BufferTime bufferTime);
    Task<bool> UpdateBufferTimeAsync(Guid id, UpdateBufferTime updateRequest);
    Task<bool> DeleteBufferTimeAsync(Guid id);
}