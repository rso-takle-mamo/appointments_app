using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Models;
using AvailabilityService.Database.UpdateModels;

namespace AvailabilityService.Database.Repositories.Interfaces;

public interface IWorkingHoursRepository
{
    Task<(IEnumerable<WorkingHours> WorkingHours, int TotalCount)> GetWorkingHoursAsync(PaginationParameters parameters, Guid? tenantId = null);
    Task<(IEnumerable<WorkingHours> WorkingHours, int TotalCount)> GetWorkingHoursByServiceAsync(Guid serviceId, PaginationParameters parameters, Guid? tenantId = null);
    Task<WorkingHours?> GetWorkingHoursByIdAsync(Guid id);
    Task CreateWorkingHoursAsync(WorkingHours workingHours);
    Task<bool> UpdateWorkingHoursAsync(Guid id, UpdateWorkingHours updateRequest);
    Task<bool> DeleteWorkingHoursAsync(Guid id);
}