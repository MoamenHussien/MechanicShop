using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.IdentityModel.Protocols;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(n => n.Id).IsClustered(false);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.Property(n => n.IssuedAtUtc).IsRequired();
        builder.Property(n => n.Status).HasConversion<string>().IsRequired();
        builder.Property(n => n.TaxAmount).HasPrecision(18, 2);
        builder.Property(n => n.DiscountAmount).HasPrecision(18, 2);
        builder.Property(n => n.PaidAt).IsRequired(false);
        builder.Navigation(n => n.InvoiceLineItems).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.OwnsMany(n => n.InvoiceLineItems, items =>
        {
            items.ToTable("Invoice Line Item");
            items.WithOwner().HasForeignKey(n => n.InvoiceId);
            items.HasKey(n => new { n.InvoiceId, n.LineNumber });
            items.Property(n => n.Description).HasMaxLength(200).IsRequired();
            items.Property(n => n.LineNumber).ValueGeneratedNever().IsRequired();
            items.Property(n => n.UnitPrice).HasPrecision(18, 2).IsRequired();
            items.Property(n => n.Quantity).IsRequired();
        });
    }
}
