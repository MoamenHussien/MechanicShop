using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryValidatorTests
{
    private readonly GetCustomerByIdQueryValidator _sut;

    public GetCustomerByIdQueryValidatorTests()
    {
        _sut = new GetCustomerByIdQueryValidator();
    }

    [Fact]
    public void GetCustomerByIdValidator_ShouldSucceed_WithValidId()
    {
        // Arrange
        var query = new GetCustomerByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.Validate(query);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void GetCustomerByIdValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var query = new GetCustomerByIdQuery(Guid.Empty);

        // Act
        var result = _sut.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CustomerId");
    }
}