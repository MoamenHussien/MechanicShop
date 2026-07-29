using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.HasKey(n => n.Id).IsClustered(false);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.HasOne(n => n.Invoice).WithOne(n => n.WorkOrder).HasForeignKey<Invoice>(n => n.WorkOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(n => n.Vehicle).WithMany(n => n.WorkOrders).HasForeignKey(n => n.VehicleId);
        builder.HasOne(n => n.Labor).WithMany().HasForeignKey(n => n.LaborId).IsRequired();
        builder.HasMany(n => n.RepairTasks).WithMany().UsingEntity(n => n.ToTable("WorkOrderRepairTasks"));
        builder.Navigation(n => n.RepairTasks).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Property(n => n.State).HasConversion<string>().IsRequired();
        builder.Property(n => n.Discount).HasPrecision(18, 2).IsRequired(false);
        builder.Property(n => n.Tax).HasPrecision(18, 2).IsRequired(false);
        builder.Property(n => n.Spot).HasConversion<string>().IsRequired();
        builder.Property(n => n.EndAtUtc).IsRequired();
        builder.Property(n => n.StartAtUtc).IsRequired();
        builder.HasIndex(a => new { a.StartAtUtc, a.EndAtUtc });
        builder.Ignore(w => w.Total);
        builder.Ignore(w => w.TotalLaborCost);
        builder.Ignore(w => w.TotalPartsCost);
        builder.Ignore(n => n.IsDeletable);
        builder.Ignore(n => n.IsEditable);

    }
}
