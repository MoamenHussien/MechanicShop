using MechanicShop.Tests.Common.Billing;
using Xunit;

public class InvoiceMapperTests
{
    [Fact]
    public void SingleLineItemToDto_WhenLineItemIsValid_ShouldMapAllProperties()
    {
        // Arrange
        var sourceLineItem = InvoiceLineItemFactory.CreateInvoiceLineItem().Value;

        // Act
        var lineItemDto = sourceLineItem.ToDto();

        // Assert
        Assert.Equal(sourceLineItem.InvoiceId, lineItemDto.InvoiceId);
        Assert.Equal(sourceLineItem.LineNumber, lineItemDto.LineNumber);
        Assert.Equal(sourceLineItem.Description, lineItemDto.Description);
        Assert.Equal(sourceLineItem.Quantity, lineItemDto.Quantity);
        Assert.Equal(sourceLineItem.UnitPrice, lineItemDto.UnitPrice);
        Assert.Equal(sourceLineItem.LineTotal, lineItemDto.LineTotal);
    }

    [Fact]
    public void GroupLineItemsToDto_WhenLineItemsAreValid_ShouldMapAllLineItems()
    {
        // Arrange
        var firstLineItem = InvoiceLineItemFactory.CreateInvoiceLineItem().Value;
        var secondLineItem = InvoiceLineItemFactory.CreateInvoiceLineItem().Value;

        List<InvoiceLineItem> sourceLineItems =
        [
            firstLineItem,
            secondLineItem
        ];

        // Act
        var lineItemDtos = sourceLineItems.ToDto();

        // Assert
        Assert.Equal(sourceLineItems.Count, lineItemDtos.Count);

        Assert.Contains(lineItemDtos, dto => dto.InvoiceId == firstLineItem.InvoiceId &&
                                             dto.LineNumber == firstLineItem.LineNumber);

        Assert.Contains(lineItemDtos, dto => dto.InvoiceId == secondLineItem.InvoiceId &&
                                             dto.LineNumber == secondLineItem.LineNumber);
    }
}
