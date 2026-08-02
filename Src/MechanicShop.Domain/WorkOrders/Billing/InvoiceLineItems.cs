using System.Runtime.CompilerServices;

public sealed class InvoiceLineItem
{
    public string Description { get; private set; }

    public int LineNumber { get; private set; }

    public decimal UnitPrice { get; private set; }

    public Guid InvoiceId { get; init; }

    public int Quantity { get; private set; }

    public decimal LineTotal => Quantity * UnitPrice;

#pragma warning disable CS8618
    private InvoiceLineItem()
    {
    }
#pragma warning restore CS8618

    public InvoiceLineItem(Guid invoiceId, string Description, int LineNumber, decimal UnitPrice, int Quantity)
    {
        this.InvoiceId = invoiceId;
        this.Description = Description;
        this.LineNumber = LineNumber;
        this.UnitPrice = UnitPrice;
        this.Quantity = Quantity;
    }

    public static Result<InvoiceLineItem> Create(Guid invoiceId, string Description, int LineNumber, decimal UnitPrice, int Quantity)
    {
        if (invoiceId == Guid.Empty)
        {
            return InvoiceLineItemErrors.InvoiceIdRequired;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            return InvoiceLineItemErrors.DescriptionRequired;
        }

        if (LineNumber <= 0)
        {
            return InvoiceLineItemErrors.LineNumberInvalid;
        }

        if (UnitPrice <= 0)
        {
            return InvoiceLineItemErrors.UnitPriceInvalid;
        }

        if (Quantity <= 0)
        {
            return InvoiceLineItemErrors.QuantityInvalid;
        }

        return new InvoiceLineItem(invoiceId, Description, LineNumber, UnitPrice, Quantity);
    }
}
