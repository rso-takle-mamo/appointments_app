using System;

namespace AvailabilityService.Database.Entities;

public enum IntegrationStatus
{
    NotConnected,
    Connected,
    Error,
    Disconnected
}

public enum SyncStatus
{
    NeverSynced,
    Success,
    Failed,
    InProgress
}

public class GoogleCalendarIntegration
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    // Connection status
    public IntegrationStatus Status { get; set; } = IntegrationStatus.NotConnected;
    public DateTime? ConnectedAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public string? DisconnectionReason { get; set; }

    // Google Calendar details
    public string? GoogleCalendarId { get; set; }
    public string[]? CalendarIdsToSync { get; set; }
    public string? GoogleUserEmail { get; set; }

    // OAuth tokens
    public string? RefreshToken { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }

    // Sync configuration
    public bool AutoSyncEnabled { get; set; } = true;
    public int SyncIntervalMinutes { get; set; } = 15;
    public DateTime? LastSyncAt { get; set; }
    public SyncStatus? LastSyncStatus { get; set; } = SyncStatus.NeverSynced;
    public string? LastSyncError { get; set; }
    public int ConsecutiveFailures { get; set; } = 0;

    // Webhook configuration
    public bool WebhookEnabled { get; set; } = false;
    public string? WebhookChannelId { get; set; }
    public string? WebhookResourceId { get; set; }
    public DateTime? WebhookExpiresAt { get; set; }
    public DateTime? WebhookLastReceivedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}