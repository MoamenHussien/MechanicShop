using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RepairTaskConfiguration : IEntityTypeConfiguration<RepairTask>
{
    public void Configure(EntityTypeBuilder<RepairTask> builder)
    {
        builder.HasKey(n=>n.Id).IsClustered(false);
        builder.Property(n=>n.Id).ValueGeneratedNever();
        builder.Property(n=>n.Name).HasMaxLength(50).IsRequired();
        builder.Property(n=>n.LaborCost).HasPrecision(18,2).IsRequired();
        builder.Property(n=>n.EstimatedDuration).HasConversion<string>().IsRequired();
        builder.HasMany(n=>n.Parts).WithOne(n=>n.RepairTask).HasForeignKey(n=>n.RepairTaskId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(n=>n.TotalPartsCost);
        builder.Ignore(n=>n.TotalCost);
    }
}