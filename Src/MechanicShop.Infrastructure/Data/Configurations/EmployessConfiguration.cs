using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasIndex(n=>n.Id).IsClustered(false);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.Property(n=>n.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(n=>n.LastName).HasMaxLength(50).IsRequired();
        builder.Property(n=>n.IsActive).IsRequired();
    }
}