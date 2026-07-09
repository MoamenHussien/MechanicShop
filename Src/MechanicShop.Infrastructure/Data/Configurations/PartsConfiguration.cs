using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PartsConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.HasKey(n=>n.Id).IsClustered(false);
        builder.Property(n=>n.Id).ValueGeneratedNever();
        builder.Property(n=>n.Costs).HasPrecision(18,2);
        builder.Property(n=>n.Quantity).IsRequired();
        builder.Property(n=>n.Name).HasMaxLength(50).IsRequired();
    }
}