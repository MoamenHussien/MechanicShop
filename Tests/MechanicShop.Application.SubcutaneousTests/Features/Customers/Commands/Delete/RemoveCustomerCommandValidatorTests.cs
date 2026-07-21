using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.Delete;

public class RemoveCustomerCommandValidatorTests
{
    private readonly DeleteCustomerCommandValidator _sut;

    public RemoveCustomerCommandValidatorTests()
    {
        _sut = new DeleteCustomerCommandValidator();
    }

    [Fact]
    public void DeleteCustomerValidator_ShouldSucceed_WithValidId()
    {
        // Arrange
        var command = new DeleteCustomerCommand(Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DeleteCustomerValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var command = new DeleteCustomerCommand(Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CustomerId");
    }
}
