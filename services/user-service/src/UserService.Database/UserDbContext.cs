using Microsoft.EntityFrameworkCore;
using UserService.Database.Entities;

namespace UserService.Database;

public class UserDbContext() : DbContext()
{
    public DbSet<User> Users { get; set; }
    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(EnvironmentVariables.GetRequiredVariable("DATABASE_CONNECTION_STRING"));
        base.OnConfiguring(optionsBuilder);
    }
    
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
            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
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