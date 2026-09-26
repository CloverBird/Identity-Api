using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guildly.Identity.Infrastructure.EntityTypeConfigurations;

public class IdentityUserConfiguration : IEntityTypeConfiguration<IdentityUser>
{
    public void Configure(EntityTypeBuilder<IdentityUser> builder)
    {
        builder.ToTable("identity_user");
        
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Email, emailBuilder =>
        {
               emailBuilder.Property(x => x.Value)
                           .HasColumnName("email")
                           .HasMaxLength(256)
                           .IsRequired();

               emailBuilder.Property(x => x.NormalizedValue)
                           .HasColumnName("normalized_email")
                           .HasMaxLength(256)
                           .IsRequired();
               
               emailBuilder.HasIndex(x => x.NormalizedValue)
                           .IsUnique();
        });
        
        builder.Property(x => x.EmailConfirmed)
               .IsRequired();
        
        builder.Property(x => x.Phone)
               .HasConversion(
                      phone => phone == null ? null : phone.Value,
                      value => value == null ? null : Phone.Create(value).Value)
               .HasMaxLength(32)
               .IsRequired(false);
        
        builder.HasIndex(x => x.Phone)
               .IsUnique();
        
        builder.Property(x => x.PhoneConfirmed)
               .IsRequired();

        builder.Property(x => x.Status)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(x => x.Role)
               .HasConversion<string>()
               .IsRequired();
        
        builder.Property(x => x.PasswordHash)
               .HasMaxLength(256)
               .IsRequired();
        
        builder.Property(x => x.CreatedAt)
               .IsRequired();
        
        builder.HasMany<VerificationSession>()
               .WithOne()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}