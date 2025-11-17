using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Models;
using AvailabilityService.Database.UpdateModels;

namespace AvailabilityService.Database.Repositories.Interfaces;

public interface IGoogleCalendarIntegrationRepository
{
    Task<(IEnumerable<GoogleCalendarIntegration> Integrations, int TotalCount)> GetIntegrationsAsync(PaginationParameters parameters, Guid? tenantId = null);
    Task<GoogleCalendarIntegration?> GetIntegrationByIdAsync(Guid id);
    Task<GoogleCalendarIntegration?> GetIntegrationByTenantIdAsync(Guid tenantId);
    Task CreateIntegrationAsync(GoogleCalendarIntegration integration);
    Task<bool> UpdateIntegrationAsync(Guid id, UpdateGoogleCalendarIntegration updateRequest);
    Task<bool> DeleteIntegrationAsync(Guid id);
}