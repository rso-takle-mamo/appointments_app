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
            entity.ToTable("categories");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");
            
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("services");
            
            entity.HasKey(e => e.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.Name).HasMaxLength(255);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(2000);
            entity.Property(x => x.Price).HasColumnName("price");
            entity.Property(x => x.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(x => x.CategoryId).HasColumnName("category_id");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("createdat");
            entity.Property(x => x.UpdatedAt).HasColumnName("updatedat");

            entity.HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}