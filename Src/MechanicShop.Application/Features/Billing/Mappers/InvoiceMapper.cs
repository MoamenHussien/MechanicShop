using System.Linq.Expressions;

public static class InvoiceMapper
{
    public static readonly Expression<Func<InvoiceLineItem, InvoiceLineItemDto>> InvoiceLineItemProjection =
        item => new InvoiceLineItemDto
        {
            InvoiceId = item.InvoiceId,
            LineNumber = item.LineNumber,
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.LineTotal,
        };

    public static readonly Expression<Func<Invoice, InvoiceDto>> InvoiceProjection =
        invoice => new InvoiceDto
        {
            InvoiceId = invoice.Id,
            WorkOrderId = invoice.WorkOrderId,
            IssuedAtUtc = invoice.IssuedAtUtc,

            Vehicle = new VehicleDto(
                invoice.WorkOrder.Vehicle.Id,
                invoice.WorkOrder.Vehicle.VehicleModel.VehicleMake.Make,
                invoice.WorkOrder.Vehicle.VehicleModel.Model,
                invoice.WorkOrder.Vehicle.Year,
                invoice.WorkOrder.Vehicle.LicensePlate),

            Customer = new CustomerDto
            {
                CustomerId = invoice.WorkOrder.Vehicle.Customer.Id,
                Name = invoice.WorkOrder.Vehicle.Customer.Name,
                Email = invoice.WorkOrder.Vehicle.Customer.Email,
                PhoneNumber = invoice.WorkOrder.Vehicle.Customer.PhoneNumber,
                Vehicles = new List<VehicleDto>(),
            },

            DiscountAmount = invoice.DiscountAmount,
            Subtotal = invoice.Subtotal,
            TaxAmount = invoice.TaxAmount,
            Total = invoice.Total,
            PaymentStatus = invoice.Status.ToString(),

            Items = invoice.InvoiceLineItems
                .Select(item => new InvoiceLineItemDto
                {
                    InvoiceId = item.InvoiceId,
                    LineNumber = item.LineNumber,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal,
                })
                .ToList(),
        };

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
            LineTotal = invoiceLine.LineTotal,
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
            Items = invoice.InvoiceLineItems.ToDto(),
        };
    }

    public static List<InvoiceDto> ToDto(this IEnumerable<Invoice> invoices)
    {
        return invoices.Select(n => n.ToDto()).ToList();
    }
}
