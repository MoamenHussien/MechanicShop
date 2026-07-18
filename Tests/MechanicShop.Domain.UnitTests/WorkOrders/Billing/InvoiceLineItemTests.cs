using Xunit;
public class InvoiceLineItemTests
{
    [Fact]
    public void CreateInvoiceLineItem_ShouldSucceed_WithValidData()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        const string description = "Brake Pad";
        const int lineNumber = 1;
        const decimal unitPrice = 50m;
        const int quantity = 2;

        // Act
        var result = InvoiceLineItem.Create(
            invoiceId,
            description,
            lineNumber,
            unitPrice,
            quantity);

        // Assert
        Assert.True(result.IsSuccess);

        var invoiceLineItem = result.Value;

        Assert.Equal(invoiceId, invoiceLineItem.InvoiceId);
        Assert.Equal(description, invoiceLineItem.Description);
        Assert.Equal(lineNumber, invoiceLineItem.LineNumber);
        Assert.Equal(unitPrice, invoiceLineItem.UnitPrice);
        Assert.Equal(quantity, invoiceLineItem.Quantity);
        Assert.Equal(100m, invoiceLineItem.LineTotal);
    }

    [Fact]
    public void CreateInvoiceLineItem_ShouldFail_WithInvalidInvoiceId()
    {
        // Act
        var result = InvoiceLineItem.Create(
            Guid.Empty,
            "Brake Pad",
            1,
            50m,
            2);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            InvoiceLineItemErrors.InvoiceIdRequired.Code,
            result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateInvoiceLineItem_ShouldFail_WithInvalidDescription(string? value)
    {
        // Act
        var result = InvoiceLineItem.Create(
            Guid.NewGuid(),
            value!,
            1,
            50m,
            2);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            InvoiceLineItemErrors.DescriptionRequired.Code,
            result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateInvoiceLineItem_ShouldFail_WithInvalidLineNumber(int value)
    {
        // Act
        var result = InvoiceLineItem.Create(
            Guid.NewGuid(),
            "Brake Pad",
            value,
            50m,
            2);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            InvoiceLineItemErrors.LineNumberInvalid.Code,
            result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateInvoiceLineItem_ShouldFail_WithInvalidUnitPrice(decimal value)
    {
        // Act
        var result = InvoiceLineItem.Create(
            Guid.NewGuid(),
            "Brake Pad",
            1,
            value,
            2);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            InvoiceLineItemErrors.UnitPriceInvalid.Code,
            result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateInvoiceLineItem_ShouldFail_WithInvalidQuantity(int value)
    {
        // Act
        var result = InvoiceLineItem.Create(
            Guid.NewGuid(),
            "Brake Pad",
            1,
            50m,
            value);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            InvoiceLineItemErrors.QuantityInvalid.Code,
            result.TopError.Code);
    }
}