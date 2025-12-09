using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCatalogService.Database.Entities;

namespace ServiceCatalogService.Database.Configurations;
  
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
  
        builder.HasKey(c => c.Id);
        
        builder.Property(t => t.Id)
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
  
        builder.Property(c => c.Description).HasMaxLength(500);

        builder.Property(c => c.TenantId).IsRequired();
        
        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();
        
        // Relationship
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => new { c.TenantId, c.Name }).IsUnique();
        builder.HasIndex(c => c.TenantId);
    }
}