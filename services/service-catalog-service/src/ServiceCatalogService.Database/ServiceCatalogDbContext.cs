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
        // Automatically set CreatedAt and UpdatedAt timestamps
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is Tenant || e.Entity is Category || e.Entity is Service &&
                (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                if (entityEntry.Entity is Tenant tenant)
                    tenant.CreatedAt = DateTime.UtcNow;
                else if (entityEntry.Entity is Category category)
                    category.CreatedAt = DateTime.UtcNow;
                else if (entityEntry.Entity is Service service)
                    service.CreatedAt = DateTime.UtcNow;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                if (entityEntry.Entity is Tenant tenant)
                    tenant.UpdatedAt = DateTime.UtcNow;
                else if (entityEntry.Entity is Category category)
                    category.UpdatedAt = DateTime.UtcNow;
                else if (entityEntry.Entity is Service service)
                    service.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}