using Microsoft.EntityFrameworkCore;
using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Models;
using AvailabilityService.Database.Repositories.Interfaces;
using AvailabilityService.Database.UpdateModels;

namespace AvailabilityService.Database.Repositories.Implementation;

public class WorkingHoursRepository(AvailabilityDbContext context) : IWorkingHoursRepository
{
    public async Task<(IEnumerable<WorkingHours> WorkingHours, int TotalCount)> GetWorkingHoursAsync(PaginationParameters parameters, Guid? tenantId = null)
    {
        var query = context.WorkingHours
            .AsNoTracking()
            .AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(wh => wh.TenantId == tenantId.Value);
        }

        var totalCount = await query.CountAsync();

        var workingHours = await query
            .OrderBy(wh => wh.Day)
            .ThenBy(wh => wh.StartTime)
            .Skip(parameters.Offset)
            .Take(parameters.Limit)
            .ToListAsync();

        return (workingHours, totalCount);
    }

    public async Task<(IEnumerable<WorkingHours> WorkingHours, int TotalCount)> GetWorkingHoursByServiceAsync(Guid serviceId, PaginationParameters parameters, Guid? tenantId = null)
    {
        var query = context.WorkingHours
            .AsNoTracking()
            .Where(wh => wh.ServiceId == serviceId);

        if (tenantId.HasValue)
        {
            query = query.Where(wh => wh.TenantId == tenantId.Value);
        }

        var totalCount = await query.CountAsync();

        var workingHours = await query
            .OrderBy(wh => wh.Day)
            .ThenBy(wh => wh.StartTime)
            .Skip(parameters.Offset)
            .Take(parameters.Limit)
            .ToListAsync();

        return (workingHours, totalCount);
    }

    public async Task<WorkingHours?> GetWorkingHoursByIdAsync(Guid id)
    {
        return await context.WorkingHours
            .AsNoTracking()
            .FirstOrDefaultAsync(wh => wh.Id == id);
    }

    public async Task CreateWorkingHoursAsync(WorkingHours workingHours)
    {
        workingHours.Id = Guid.NewGuid();
        workingHours.CreatedAt = DateTime.UtcNow;
        workingHours.UpdatedAt = DateTime.UtcNow;

        await context.WorkingHours.AddAsync(workingHours);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateWorkingHoursAsync(Guid id, UpdateWorkingHours updateRequest)
    {
        var existingWorkingHours = await context.WorkingHours.FindAsync(id);
        if (existingWorkingHours == null) return false;

        if (updateRequest.ServiceId.HasValue)
            existingWorkingHours.ServiceId = updateRequest.ServiceId.Value;

        if (updateRequest.Day.HasValue)
            existingWorkingHours.Day = updateRequest.Day.Value;

        if (updateRequest.StartTime.HasValue)
            existingWorkingHours.StartTime = updateRequest.StartTime.Value;

        if (updateRequest.EndTime.HasValue)
            existingWorkingHours.EndTime = updateRequest.EndTime.Value;

        if (updateRequest.IsActive.HasValue)
            existingWorkingHours.IsActive = updateRequest.IsActive.Value;

        if (updateRequest.MaxConcurrentBookings.HasValue)
            existingWorkingHours.MaxConcurrentBookings = updateRequest.MaxConcurrentBookings.Value;

        existingWorkingHours.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteWorkingHoursAsync(Guid id)
    {
        var workingHours = await context.WorkingHours.FindAsync(id);
        if (workingHours == null) return false;

        context.WorkingHours.Remove(workingHours);
        await context.SaveChangesAsync();
        return true;
    }
}