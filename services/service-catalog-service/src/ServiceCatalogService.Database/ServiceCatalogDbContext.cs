using Microsoft.EntityFrameworkCore;
using ServiceCatalogService.Database.Entities;

namespace ServiceCatalogService.Database;

public class ServiceCatalogDbContext() : DbContext()
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Service> Services { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(EnvironmentVariables.GetRequiredVariable("DATABASE_CONNECTION_STRING"));
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).
                IsRequired().
                HasMaxLength(100);
            entity.Property(e => e.Description).
                HasMaxLength(500);
            entity.Property(e => e.TenantId)
                .IsRequired();

            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.HasIndex(e => e.TenantId);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).
                IsRequired().
                HasMaxLength(255);
            entity.Property(e => e.Description).
                HasMaxLength(1000);
            entity.Property(e => e.TenantId)
                .IsRequired();
            entity.Property(e => e.Price)
                .HasPrecision(10, 2);
            entity.Property(e => e.DurationMinutes)
                .HasDefaultValue(30);
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);
            
            entity.HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            

            entity.ToTable(table => table.HasCheckConstraint(
                "CK_Service_Positive_Price",
                "\"Price\" >= 0"
            ));
            
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_Service_Positive_Duration",
                "\"DurationMinutes\" > 0"
            ));

            entity.ToTable(table => table.HasCheckConstraint(
                "CK_Service_Reasonable_Duration",
                "\"DurationMinutes\" <= 480"
            ));
        });
    }
}