using Microsoft.EntityFrameworkCore;
using AvailabilityService.Database.Entities;

namespace AvailabilityService.Database;

public class AvailabilityDbContext : DbContext
{
    public DbSet<WorkingHours> WorkingHours { get; set; }
    public DbSet<TimeBlock> TimeBlocks { get; set; }
    public DbSet<GoogleCalendarIntegration> GoogleCalendarIntegrations { get; set; }
    public DbSet<BufferTime> BufferTimes { get; set; }
    public DbSet<TenantSettings> TenantSettings { get; set; }

    public AvailabilityDbContext() { }

    public AvailabilityDbContext(DbContextOptions<AvailabilityDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(EnvironmentVariables.GetRequiredVariable("DATABASE_CONNECTION_STRING"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // WorkingHours configuration
        modelBuilder.Entity<WorkingHours>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.ServiceId);
            entity.Property(e => e.Day).IsRequired();
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.MaxConcurrentBookings).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => new { e.TenantId, e.Day }).IsUnique();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.ServiceId);
        });

        // TimeBlock configuration
        modelBuilder.Entity<TimeBlock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.StartDateTime).IsRequired();
            entity.Property(e => e.EndDateTime).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Reason);
            entity.Property(e => e.IsRecurring).IsRequired();
            entity.Property(e => e.Pattern).IsRequired();
            entity.Property(e => e.RecurringDays);
            entity.Property(e => e.RecurrenceEndDate);
            entity.Property(e => e.ExternalEventId);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => new { e.TenantId, e.StartDateTime });
            entity.HasIndex(e => new { e.TenantId, e.EndDateTime });
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.ExternalEventId);
        });

        // GoogleCalendarIntegration configuration
        modelBuilder.Entity<GoogleCalendarIntegration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.ConnectedAt);
            entity.Property(e => e.DisconnectedAt);
            entity.Property(e => e.DisconnectionReason);
            entity.Property(e => e.GoogleCalendarId);
            entity.Property(e => e.CalendarIdsToSync);
            entity.Property(e => e.GoogleUserEmail);
            entity.Property(e => e.RefreshToken);
            entity.Property(e => e.AccessToken);
            entity.Property(e => e.TokenExpiresAt);
            entity.Property(e => e.AutoSyncEnabled).IsRequired();
            entity.Property(e => e.SyncIntervalMinutes).IsRequired();
            entity.Property(e => e.LastSyncAt);
            entity.Property(e => e.LastSyncStatus);
            entity.Property(e => e.LastSyncError);
            entity.Property(e => e.ConsecutiveFailures).IsRequired();
            entity.Property(e => e.WebhookEnabled).IsRequired();
            entity.Property(e => e.WebhookChannelId);
            entity.Property(e => e.WebhookResourceId);
            entity.Property(e => e.WebhookExpiresAt);
            entity.Property(e => e.WebhookLastReceivedAt);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.TenantId).IsUnique();
        });

        // BufferTime configuration
        modelBuilder.Entity<BufferTime>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.BeforeMinutes).IsRequired();
            entity.Property(e => e.AfterMinutes).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.TenantId).IsUnique();
        });

        // TenantSettings configuration
        modelBuilder.Entity<TenantSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.TimeZone).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.TenantId).IsUnique();
        });
    }
}