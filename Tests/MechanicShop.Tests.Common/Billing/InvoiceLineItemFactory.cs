namespace MechanicShop.Tests.Common.Billing;

public static class InvoiceLineItemFactory
{
    public static Result<InvoiceLineItem> CreateInvoiceLineItem(
        Guid? id = null,
        int? lineNumber = null,
        string? description = null,
        int? quantity = null,
        decimal? unitPrice = null)
    {
        return InvoiceLineItem.Create(
            id ?? Guid.NewGuid(),
            description ?? "some invoice line",
            lineNumber ?? 1,
            unitPrice ?? 100m,
            quantity ?? 1);
    }
}
