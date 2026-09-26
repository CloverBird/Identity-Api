using Guildly.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guildly.Identity.Infrastructure.EntityTypeConfigurations;

internal class VerificationSessionConfiguration : IEntityTypeConfiguration<VerificationSession>
{
    public void Configure(EntityTypeBuilder<VerificationSession> builder)
    {
        // TODO: Add lookup indexes for verification sessions
        builder.ToTable("verification_session");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.CodeHash)
               .HasMaxLength(64)
               .IsRequired();
        
        builder.Property(x => x.Channel)
               .HasConversion<string>()
               .IsRequired();
        
        builder.Property(x => x.Purpose)
               .HasConversion<string>()
               .IsRequired();
        
        builder.Property(x => x.SentTo)
               .HasMaxLength(256)
               .IsRequired();
        
        builder.Property(x => x.ExpiresAt)
               .IsRequired();
        
        builder.Property(x => x.CreatedAt)
               .IsRequired();
        
        builder.Property(x => x.UsedAt)
               .IsRequired(false);
        
        builder.Property(x => x.CancelledAt)
               .IsRequired(false);
        
        builder.Property(x => x.FailedAttempts)
               .IsRequired();
    }
}