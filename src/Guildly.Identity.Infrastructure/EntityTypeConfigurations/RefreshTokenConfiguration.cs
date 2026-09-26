using Guildly.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guildly.Identity.Infrastructure.EntityTypeConfigurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.TokenHash)
               .HasMaxLength(64)
               .IsRequired();
        
        builder.Property(x => x.CreatedAt)
               .IsRequired();
        
        builder.Property(x => x.ExpiresAt)
               .IsRequired();
        
        builder.Property(x => x.RevokedAt)
               .IsRequired(false);

        builder.HasOne<IdentityUser>()
               .WithMany()
               .HasForeignKey(x => x.UserId);
    }
}