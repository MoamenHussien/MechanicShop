using System.Runtime.CompilerServices;

public static class InvoiceMapper
{
    public static InvoiceLineItemDto ToDto(this InvoiceLineItem invoiceLine)
    {
        ArgumentNullException.ThrowIfNull(invoiceLine);
        return new InvoiceLineItemDto
        {
            InvoiceId = invoiceLine.InvoiceId,
            LineNumber = invoiceLine.LineNumber,
            Description = invoiceLine.Description,
            Quantity = invoiceLine.Quantity,
            UnitPrice = invoiceLine.UnitPrice,
            LineTotal = invoiceLine.LineTotal
        };
    }

    public static List<InvoiceLineItemDto> ToDto(this IEnumerable<InvoiceLineItem> invoiceLines)
    {
        return invoiceLines.Select(n => n.ToDto()).ToList();
    }

    public static InvoiceDto ToDto(this Invoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        return new InvoiceDto
        {
            InvoiceId = invoice.Id,
            WorkOrderId = invoice.WorkOrderId,
            IssuedAtUtc = invoice.IssuedAtUtc,
            Vehicle = invoice.WorkOrder.Vehicle.ToDto(),
            Customer = invoice.WorkOrder.Vehicle.Customer.ToDto(),
            DiscountAmount = invoice.DiscountAmount,
            Subtotal = invoice.Subtotal,
            TaxAmount = invoice.TaxAmount,
            Total = invoice.Total,
            PaymentStatus = invoice.Status.ToString(),
            Items = invoice.InvoiceLineItems.ToDto()
        };
    }

    public static List<InvoiceDto> ToDto(this IEnumerable<Invoice> invoices)
    {
        return invoices.Select(n => n.ToDto()).ToList();
    }
}