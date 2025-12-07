using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Database.Entities;
  
namespace UserService.Database.Configurations;
  
public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");
  
        builder.HasKey(us => us.Id);
  
        builder.Property(us => us.Id)
            .HasDefaultValueSql("gen_random_uuid()");
  
        builder.Property(us => us.UserId)
            .IsRequired();
  
        builder.Property(us => us.TokenJti)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(us => us.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();
  
        // Indexes
        builder.HasIndex(us => us.UserId);
        builder.HasIndex(us => us.TokenJti).IsUnique();
        builder.HasIndex(us => us.ExpiresAt);
        builder.HasIndex(us => new { us.UserId, us.ExpiresAt });
    }
}