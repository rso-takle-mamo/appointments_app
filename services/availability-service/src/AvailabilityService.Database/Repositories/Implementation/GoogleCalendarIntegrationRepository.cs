using Microsoft.EntityFrameworkCore;
using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Models;
using AvailabilityService.Database.Repositories.Interfaces;
using AvailabilityService.Database.UpdateModels;

namespace AvailabilityService.Database.Repositories.Implementation;

public class GoogleCalendarIntegrationRepository(AvailabilityDbContext context) : IGoogleCalendarIntegrationRepository
{
    public async Task<(IEnumerable<GoogleCalendarIntegration> Integrations, int TotalCount)> GetIntegrationsAsync(PaginationParameters parameters, Guid? tenantId = null)
    {
        var query = context.GoogleCalendarIntegrations
            .AsNoTracking()
            .AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(gci => gci.TenantId == tenantId.Value);
        }

        var totalCount = await query.CountAsync();

        var integrations = await query
            .OrderBy(gci => gci.CreatedAt)
            .Skip(parameters.Offset)
            .Take(parameters.Limit)
            .ToListAsync();

        return (integrations, totalCount);
    }

    public async Task<GoogleCalendarIntegration?> GetIntegrationByIdAsync(Guid id)
    {
        return await context.GoogleCalendarIntegrations
            .AsNoTracking()
            .FirstOrDefaultAsync(gci => gci.Id == id);
    }

    public async Task<GoogleCalendarIntegration?> GetIntegrationByTenantIdAsync(Guid tenantId)
    {
        return await context.GoogleCalendarIntegrations
            .AsNoTracking()
            .FirstOrDefaultAsync(gci => gci.TenantId == tenantId);
    }

    public async Task CreateIntegrationAsync(GoogleCalendarIntegration integration)
    {
        integration.Id = Guid.NewGuid();
        integration.CreatedAt = DateTime.UtcNow;
        integration.UpdatedAt = DateTime.UtcNow;

        await context.GoogleCalendarIntegrations.AddAsync(integration);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateIntegrationAsync(Guid id, UpdateGoogleCalendarIntegration updateRequest)
    {
        var existingIntegration = await context.GoogleCalendarIntegrations.FindAsync(id);
        if (existingIntegration == null) return false;

        if (updateRequest.Status.HasValue)
            existingIntegration.Status = updateRequest.Status.Value;

        if (updateRequest.Status == IntegrationStatus.Connected && !existingIntegration.ConnectedAt.HasValue)
            existingIntegration.ConnectedAt = DateTime.UtcNow;
        else if (updateRequest.Status == IntegrationStatus.Disconnected && !existingIntegration.DisconnectedAt.HasValue)
            existingIntegration.DisconnectedAt = DateTime.UtcNow;

        if (updateRequest.GoogleCalendarId != null)
            existingIntegration.GoogleCalendarId = updateRequest.GoogleCalendarId;

        if (updateRequest.CalendarIdsToSync != null)
            existingIntegration.CalendarIdsToSync = updateRequest.CalendarIdsToSync;

        if (updateRequest.GoogleUserEmail != null)
            existingIntegration.GoogleUserEmail = updateRequest.GoogleUserEmail;

        if (updateRequest.RefreshToken != null)
            existingIntegration.RefreshToken = updateRequest.RefreshToken;

        if (updateRequest.AccessToken != null)
            existingIntegration.AccessToken = updateRequest.AccessToken;

        if (updateRequest.TokenExpiresAt.HasValue)
            existingIntegration.TokenExpiresAt = updateRequest.TokenExpiresAt.Value;

        if (updateRequest.AutoSyncEnabled.HasValue)
            existingIntegration.AutoSyncEnabled = updateRequest.AutoSyncEnabled.Value;

        if (updateRequest.SyncIntervalMinutes.HasValue)
            existingIntegration.SyncIntervalMinutes = updateRequest.SyncIntervalMinutes.Value;

        if (updateRequest.LastSyncAt.HasValue)
            existingIntegration.LastSyncAt = updateRequest.LastSyncAt.Value;

        if (updateRequest.LastSyncStatus.HasValue)
            existingIntegration.LastSyncStatus = updateRequest.LastSyncStatus.Value;

        if (updateRequest.LastSyncError != null)
            existingIntegration.LastSyncError = updateRequest.LastSyncError;

        if (updateRequest.ConsecutiveFailures.HasValue)
            existingIntegration.ConsecutiveFailures = updateRequest.ConsecutiveFailures.Value;

        if (updateRequest.WebhookEnabled.HasValue)
            existingIntegration.WebhookEnabled = updateRequest.WebhookEnabled.Value;

        if (updateRequest.WebhookChannelId != null)
            existingIntegration.WebhookChannelId = updateRequest.WebhookChannelId;

        if (updateRequest.WebhookResourceId != null)
            existingIntegration.WebhookResourceId = updateRequest.WebhookResourceId;

        if (updateRequest.WebhookExpiresAt.HasValue)
            existingIntegration.WebhookExpiresAt = updateRequest.WebhookExpiresAt.Value;

        if (updateRequest.WebhookLastReceivedAt.HasValue)
            existingIntegration.WebhookLastReceivedAt = updateRequest.WebhookLastReceivedAt.Value;

        existingIntegration.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteIntegrationAsync(Guid id)
    {
        var integration = await context.GoogleCalendarIntegrations.FindAsync(id);
        if (integration == null) return false;

        context.GoogleCalendarIntegrations.Remove(integration);
        await context.SaveChangesAsync();
        return true;
    }
}