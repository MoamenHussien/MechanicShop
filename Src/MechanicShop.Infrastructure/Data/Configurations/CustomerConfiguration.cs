using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id).IsClustered(false);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.Property(n=>n.Name).HasMaxLength(50).IsRequired();
        builder.Property(n=>n.Email).HasMaxLength(150);
        builder.HasMany(n=>n.vehicles).WithOne(n=>n.Customer).HasForeignKey(n=>n.CustomerId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        builder.Property(n=>n.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Navigation(n=>n.vehicles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}