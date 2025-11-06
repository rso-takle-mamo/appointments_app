using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using UserService.Api.Models.Entities;

namespace UserService.Api.Data;

public static class UserRoleExtensions
{
    public static string ToPostgresValue(this UserRole role) => role switch
    {
        UserRole.Provider => "Provider",
        UserRole.Customer => "Customer",
        _ => throw new ArgumentException($"Unknown UserRole: {role}")
    };

    public static UserRole FromPostgresValue(string value) => value switch
    {
        "Provider" => UserRole.Provider,
        "Customer" => UserRole.Customer,
        _ => throw new ArgumentException($"Unknown UserRole value: {value}")
    };
}

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.TenantId);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Username).HasColumnName("username").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Password).HasColumnName("password").IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).HasColumnName("firstname").IsRequired().HasMaxLength(255);
            entity.Property(e => e.LastName).HasColumnName("lastname").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.TenantId).HasColumnName("tenantid");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");

            entity.HasOne(u => u.Tenant)
                .WithMany()
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OwnerId).IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OwnerId).HasColumnName("ownerid").IsRequired();
            entity.Property(e => e.BusinessName).HasColumnName("businessname").IsRequired().HasMaxLength(255);
            entity.Property(e => e.BusinessEmail).HasColumnName("businessemail").HasMaxLength(255);
            entity.Property(e => e.BusinessPhone).HasColumnName("businessphone").HasMaxLength(50);
            entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(500);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");

            entity.HasOne(t => t.Owner)
                .WithOne()
                .HasForeignKey<Tenant>(t => t.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}