using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(n => n.Id).IsClustered(false);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.Property(n => n.UserId).IsRequired();
        builder.Property(n => n.Token).IsRequired().HasMaxLength(125);
        builder.HasIndex(n => n.Token).IsUnique();
        builder.Property(n => n.ExpiresOnUtc).IsRequired();
        builder.Property(n => n.RevokedOn).IsRequired(false);
    }
}
