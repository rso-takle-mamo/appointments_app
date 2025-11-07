using Microsoft.EntityFrameworkCore;
using ServiceCatalogService.Api.Models.Entities;

namespace ServiceCatalogService.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Service> Services { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure table names to match quoted names in PostgreSQL
        modelBuilder.Entity<Category>()
            .ToTable("Categories");

        modelBuilder.Entity<Service>()
            .ToTable("Services");

        // Add unique constraint for category names
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // Relationship configuration
        modelBuilder.Entity<Service>()
            .HasOne(s => s.Category)
            .WithMany()
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}