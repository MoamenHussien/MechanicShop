using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class VehicleModelConfiguration : IEntityTypeConfiguration<VehicleModel>
{
    public void Configure(EntityTypeBuilder<VehicleModel> builder)
    {
       builder.HasKey(n=>n.Id).IsClustered(false);
       builder.Property(n => n.Id).ValueGeneratedNever();
       builder.Property(n=>n.Model).HasMaxLength(40).IsRequired();
       builder.HasIndex(n=> new {n.VehicleMakeId,n.Model}).IsUnique();
       builder.Navigation(n=>n.Vehicles).UsePropertyAccessMode(PropertyAccessMode.Field);
       builder.HasMany(n=>n.Vehicles).WithOne(n=>n.VehicleModel).HasForeignKey(n=>n.VehicleModelId);
    }
}
