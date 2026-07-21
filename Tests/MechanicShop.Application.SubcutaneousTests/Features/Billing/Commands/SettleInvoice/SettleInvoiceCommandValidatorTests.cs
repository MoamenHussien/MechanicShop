using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.SettleInvoice;

public class SettleInvoiceCommandValidatorTests
{
    private readonly SettleInvoiceCommandValidator _sut;

    public SettleInvoiceCommandValidatorTests()
    {
        _sut = new SettleInvoiceCommandValidator();
    }

    [Fact]
    public void SettleInvoiceValidator_ShouldSucceed_WithValidId()
    {
        // Arrange
        var command = new SettleInvoiceCommand(Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void SettleInvoiceValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var command = new SettleInvoiceCommand(Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "InvoiceId");
    }
}