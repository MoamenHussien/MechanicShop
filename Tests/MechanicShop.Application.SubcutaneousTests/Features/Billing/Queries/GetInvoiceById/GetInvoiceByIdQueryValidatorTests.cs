using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryValidatorTests
{
    private readonly GetInvoiceByIdQueryValidator _sut;

    public GetInvoiceByIdQueryValidatorTests()
    {
        _sut = new GetInvoiceByIdQueryValidator();
    }

    [Fact]
    public void GetInvoiceByIdValidator_ShouldSucceed_WithValidId()
    {
        // Arrange
        var query = new GetInvoiceByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.Validate(query);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void GetInvoiceByIdValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var query = new GetInvoiceByIdQuery(Guid.Empty);

        // Act
        var result = _sut.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "invoiceId");
    }
}
