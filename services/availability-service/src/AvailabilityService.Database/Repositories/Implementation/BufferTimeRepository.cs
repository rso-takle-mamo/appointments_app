using Microsoft.EntityFrameworkCore;
using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Models;
using AvailabilityService.Database.Repositories.Interfaces;
using AvailabilityService.Database.UpdateModels;

namespace AvailabilityService.Database.Repositories.Implementation;

public class BufferTimeRepository(AvailabilityDbContext context) : IBufferTimeRepository
{
    public async Task<(IEnumerable<BufferTime> BufferTimes, int TotalCount)> GetBufferTimesAsync(PaginationParameters parameters, Guid? tenantId = null)
    {
        var query = context.BufferTimes
            .AsNoTracking()
            .AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(bt => bt.TenantId == tenantId.Value);
        }

        var totalCount = await query.CountAsync();

        var bufferTimes = await query
            .OrderBy(bt => bt.CreatedAt)
            .Skip(parameters.Offset)
            .Take(parameters.Limit)
            .ToListAsync();

        return (bufferTimes, totalCount);
    }

    public async Task<BufferTime?> GetBufferTimeByIdAsync(Guid id)
    {
        return await context.BufferTimes
            .AsNoTracking()
            .FirstOrDefaultAsync(bt => bt.Id == id);
    }

    public async Task<BufferTime?> GetBufferTimeByTenantIdAsync(Guid tenantId)
    {
        return await context.BufferTimes
            .AsNoTracking()
            .FirstOrDefaultAsync(bt => bt.TenantId == tenantId);
    }

    public async Task CreateBufferTimeAsync(BufferTime bufferTime)
    {
        bufferTime.Id = Guid.NewGuid();
        bufferTime.CreatedAt = DateTime.UtcNow;
        bufferTime.UpdatedAt = DateTime.UtcNow;

        await context.BufferTimes.AddAsync(bufferTime);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateBufferTimeAsync(Guid id, UpdateBufferTime updateRequest)
    {
        var existingBufferTime = await context.BufferTimes.FindAsync(id);
        if (existingBufferTime == null) return false;

        if (updateRequest.BeforeMinutes.HasValue)
            existingBufferTime.BeforeMinutes = updateRequest.BeforeMinutes.Value;

        if (updateRequest.AfterMinutes.HasValue)
            existingBufferTime.AfterMinutes = updateRequest.AfterMinutes.Value;

        existingBufferTime.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBufferTimeAsync(Guid id)
    {
        var bufferTime = await context.BufferTimes.FindAsync(id);
        if (bufferTime == null) return false;

        context.BufferTimes.Remove(bufferTime);
        await context.SaveChangesAsync();
        return true;
    }
}