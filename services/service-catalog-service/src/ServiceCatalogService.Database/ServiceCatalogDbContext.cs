using Microsoft.EntityFrameworkCore;
using ServiceCatalogService.Database.Configurations;
using ServiceCatalogService.Database.Entities;

namespace ServiceCatalogService.Database;

public class ServiceCatalogDbContext : DbContext
{
    public ServiceCatalogDbContext() { }

    public ServiceCatalogDbContext(DbContextOptions<ServiceCatalogDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Service> Services { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(EnvironmentVariables.GetRequiredVariable("DATABASE_CONNECTION_STRING"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceConfiguration());
        
        // Data replication from users service
        // TODO add kafka to synchronize it
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is Tenant || e.Entity is Category || e.Entity is Service &&
                (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            switch (entityEntry.State)
            {
                case EntityState.Added:
                    ((dynamic)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    ((dynamic)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}