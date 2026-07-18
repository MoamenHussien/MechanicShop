using MechanicShop.Tests.Common;
using MechanicShop.Tests.Common.Billing;
using Xunit;

public class InvoiceTests
{
    [Fact]
    public void CreateInvoice_ShouldSucceed_WithValidData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var workOrderId = Guid.NewGuid();

        var time = new FakeTimeProvider();
        time.SetUtcNow(DateTimeOffset.Parse("2026-01-01T00:00:00Z")); // to fake the time now is 2026-01-01T00:00:00Z

        List<InvoiceLineItem> invoiceLineItems =
        [
            InvoiceLineItemFactory.CreateInvoiceLineItem(
                unitPrice: 50m,
                quantity: 2).Value
        ];

        // Act
        var result = InvoiceFactory.CreateInvoice(
            id: id,
            workOrderId: workOrderId,
            items: invoiceLineItems,
            taxAmount: 5,
            discount: 10,
            timeProvider: time);

        // Assert
        Assert.True(result.IsSuccess);

        var invoice = result.Value;

        Assert.Equal(id, invoice.Id);
        Assert.Equal(workOrderId, invoice.WorkOrderId);
        Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);

        Assert.Equal(5m, invoice.TaxAmount);
        Assert.Equal(10m, invoice.DiscountAmount);

        Assert.Equal(100m, invoice.Subtotal);
        Assert.Equal(95m, invoice.Total);

        Assert.Equal(time.GetUtcNow(), invoice.IssuedAtUtc); // here to Check if the same time is 2026-01-01T00:00:00Z
    }

    [Fact]
    public void CreateInvoice_ShouldSucceed_WithEmptyId()
    {
        // Act
        var result = InvoiceFactory.CreateInvoice(id: Guid.Empty);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public void CreateInvoice_ShouldFail_WithInvalidWorkOrderId()
    {
        // Act
        var result = InvoiceFactory.CreateInvoice(
            workOrderId: Guid.Empty);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            InvoiceErrors.WorkOrderIdInvalid.Code,
            result.TopError.Code);
    }

    [Fact]
    public void CreateInvoice_ShouldFail_WithInvalidInvoiceLineItems()
    {
        // Act
        var result = InvoiceFactory.CreateInvoice(
            items: null);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            InvoiceErrors.LineItemsEmpty.Code,
            result.TopError.Code);
    }

    [Fact]
    public void CreateInvoice_ShouldFail_WithEmptyInvoiceLineItems()
    {
        // Act
        var result = InvoiceFactory.CreateInvoice(
            items: []);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            InvoiceErrors.LineItemsEmpty.Code,
            result.TopError.Code);
    }

    [Fact]
    public void ApplyDiscount_ShouldSucceed_WithValidData()
    {
        // Arrange
        var invoice = InvoiceFactory.CreateInvoice().Value;

        // Act
        var result = invoice.ApplyDiscount(20m);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(20m, invoice.DiscountAmount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void ApplyDiscount_ShouldFail_WithInvalidDiscount(decimal value)
    {
        // Arrange
        var invoice = InvoiceFactory.CreateInvoice().Value;

        // Act
        var result = invoice.ApplyDiscount(value);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            InvoiceErrors.DiscountNegative.Code,
            result.TopError.Code);
    }

    [Fact]
    public void ApplyDiscount_ShouldFail_WhenDiscountExceedsSubtotal()
    {
        // Arrange
        var invoice = InvoiceFactory.CreateInvoice().Value;

        // Act
        var result = invoice.ApplyDiscount(invoice.Subtotal);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            InvoiceErrors.DiscountExceedsSubtotal.Code,
            result.TopError.Code);
    }

    [Fact]
    public void ApplyDiscount_ShouldFail_WhenInvoiceIsPaid()
    {
        // Arrange
        var invoice = InvoiceFactory.CreateInvoice().Value;

        invoice.MarkAsPaid(TimeProvider.System);

        // Act
        var result = invoice.ApplyDiscount(10m);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            InvoiceErrors.InvoiceLocked.Code,
            result.TopError.Code);
    }

    [Fact]
    public void MarkAsPaid_ShouldSucceed_WithValidData()
    {
        // Arrange
        var invoice = InvoiceFactory.CreateInvoice().Value;

        var time = new FakeTimeProvider();
        time.SetUtcNow(DateTimeOffset.Parse("2026-01-01T00:00:00Z"));

        // Act
        var result = invoice.MarkAsPaid(time);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(time.GetUtcNow(), invoice.PaidAt); // to check if the both are the same date 
    }

    [Fact]
    public void MarkAsPaid_ShouldFail_WhenInvoiceIsAlreadyPaid()
    {
        // Arrange
        var invoice = InvoiceFactory.CreateInvoice().Value;

        invoice.MarkAsPaid(TimeProvider.System);

        // Act
        var result = invoice.MarkAsPaid(TimeProvider.System);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            InvoiceErrors.InvoiceLocked.Code,
            result.TopError.Code);
    }
}