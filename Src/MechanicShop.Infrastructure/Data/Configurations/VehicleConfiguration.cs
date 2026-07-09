using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(n=>n.Id).IsClustered(false);
        builder.Property(v => v.Id).ValueGeneratedNever();
        builder.Property(n=>n.Year).IsRequired();
        builder.Property(n=>n.LicensePlate).HasMaxLength(15).IsRequired();
        builder.HasOne(n=>n.VehicleModel).WithMany(n=>n.Vehicles).HasForeignKey(n=>n.VehicleModelId).IsRequired();
        builder.HasOne(n=>n.Customer).WithMany(n=>n.vehicles).HasForeignKey(n=>n.CustomerId).IsRequired();
    }
}