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
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}