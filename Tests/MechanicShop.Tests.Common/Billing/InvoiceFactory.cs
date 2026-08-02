namespace MechanicShop.Tests.Common.Billing;

public static class InvoiceFactory
{
    public static Result<Invoice> CreateInvoice(
        Guid? id = null,
        Guid? workOrderId = null,
        List<InvoiceLineItem>? items = null,
        decimal? discount = null,
        decimal? taxAmount = null,
        TimeProvider? timeProvider = null)
    {
        return Invoice.Create(id ?? Guid.NewGuid(), timeProvider ?? TimeProvider.System, taxAmount ?? 0, discount ?? 0, items ?? [InvoiceLineItem.Create(Guid.NewGuid(), "Oil Change", 1, 50, 2).Value], workOrderId ?? Guid.NewGuid());
    }
}
