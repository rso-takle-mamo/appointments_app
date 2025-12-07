using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Models;
using AvailabilityService.Database.UpdateModels;

namespace AvailabilityService.Database.Repositories.Interfaces;

public interface ITimeBlockRepository
{
    Task<(IEnumerable<TimeBlock> TimeBlocks, int TotalCount)> GetTimeBlocksAsync(PaginationParameters parameters, Guid? tenantId = null);
    Task<(IEnumerable<TimeBlock> TimeBlocks, int TotalCount)> GetTimeBlocksByDateRangeAsync(DateTime start, DateTime end, Guid? tenantId = null);
    Task<TimeBlock?> GetTimeBlockByIdAsync(Guid id);
    Task<IEnumerable<TimeBlock>> GetTimeBlocksByTenantAsync(Guid tenantId);
    Task CreateTimeBlockAsync(TimeBlock timeBlock);
    Task<bool> UpdateTimeBlockAsync(Guid id, UpdateTimeBlock updateRequest);
    Task<bool> DeleteTimeBlockAsync(Guid id);
    Task<int> DeleteTimeBlocksByDateRangeAsync(DateTime start, DateTime end, Guid tenantId);
}