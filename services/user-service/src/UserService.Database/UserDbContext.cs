using Microsoft.EntityFrameworkCore;
using UserService.Database.Entities;

namespace UserService.Database;

public class UserDbContext() : DbContext()
{
    public DbSet<User> Users { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(EnvironmentVariables.GetRequiredVariable("DATABASE_CONNECTION_STRING"));
        base.OnConfiguring(optionsBuilder);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.TenantId);

            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OwnerId).IsUnique();

            entity.Property(e => e.OwnerId)
                .IsRequired();
            entity.Property(e => e.BusinessName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.BusinessEmail)
                .HasMaxLength(255);
            entity.Property(e => e.BusinessPhone)
                .HasMaxLength(50);
            entity.Property(e => e.Address)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TokenJti).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
        });
    }
}