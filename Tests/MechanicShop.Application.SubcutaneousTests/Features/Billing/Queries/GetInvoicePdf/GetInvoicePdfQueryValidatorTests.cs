using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoicePdf;

public class GetInvoicePdfQueryValidatorTests
{
    private readonly GetInvoicePdfQueryValidator _sut;

    public GetInvoicePdfQueryValidatorTests()
    {
        _sut = new GetInvoicePdfQueryValidator();
    }

    [Fact]
    public void GetInvoicePdfValidator_ShouldSucceed_WithValidId()
    {
        // Arrange
        var query = new GetInvoicePdfQuery(Guid.NewGuid());

        // Act
        var result = _sut.Validate(query);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void GetInvoicePdfValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var query = new GetInvoicePdfQuery(Guid.Empty);

        // Act
        var result = _sut.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "InvoiceId");
    }
}