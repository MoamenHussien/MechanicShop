using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class VehicleMakeConfiguration : IEntityTypeConfiguration<VehicleMake>
{
    public void Configure(EntityTypeBuilder<VehicleMake> builder)
    {
        builder.HasKey(n=>n.Id).IsClustered(false);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.HasMany(n=>n.VehicleModels).WithOne(n=>n.VehicleMake).HasForeignKey(n=>n.VehicleMakeId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        builder.Property(n=>n.Make).HasMaxLength(20).IsRequired();
        builder.Navigation(n=>n.VehicleModels).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(n => n.Make).IsUnique();
    }
}