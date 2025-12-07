using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCatalogService.Database.Entities;

namespace ServiceCatalogService.Database.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.BusinessName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.CreatedAt);

        builder.Property(t => t.UpdatedAt);
    }
}