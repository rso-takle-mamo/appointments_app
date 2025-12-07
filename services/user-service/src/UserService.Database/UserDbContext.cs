using Microsoft.EntityFrameworkCore;
using UserService.Database.Entities;
using UserService.Database.Configurations;

namespace UserService.Database;

public class UserDbContext : DbContext
{
    public UserDbContext() { }

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from separate classes
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set UpdatedAt timestamp
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is User || e.Entity is Tenant &&
                (e.State == EntityState.Added || e.State == EntityState.Modified));
  
        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                if (entityEntry.Entity is User user)
                    user.CreatedAt = DateTime.UtcNow;
                else if (entityEntry.Entity is Tenant tenant)
                    tenant.CreatedAt = DateTime.UtcNow;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                if (entityEntry.Entity is User user)
                    user.UpdatedAt = DateTime.UtcNow;
                else if (entityEntry.Entity is Tenant tenant)
                    tenant.UpdatedAt = DateTime.UtcNow;
            }
        }
  
        return await base.SaveChangesAsync(cancellationToken);
    }
}