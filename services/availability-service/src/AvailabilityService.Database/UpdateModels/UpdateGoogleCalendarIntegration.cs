using AvailabilityService.Database.Entities;

namespace AvailabilityService.Database.UpdateModels;

public class UpdateGoogleCalendarIntegration
{
    public IntegrationStatus? Status { get; set; }
    public string? GoogleCalendarId { get; set; }
    public string[]? CalendarIdsToSync { get; set; }
    public string? GoogleUserEmail { get; set; }
    public string? RefreshToken { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public bool? AutoSyncEnabled { get; set; }
    public int? SyncIntervalMinutes { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public SyncStatus? LastSyncStatus { get; set; }
    public string? LastSyncError { get; set; }
    public int? ConsecutiveFailures { get; set; }
    public bool? WebhookEnabled { get; set; }
    public string? WebhookChannelId { get; set; }
    public string? WebhookResourceId { get; set; }
    public DateTime? WebhookExpiresAt { get; set; }
    public DateTime? WebhookLastReceivedAt { get; set; }
}