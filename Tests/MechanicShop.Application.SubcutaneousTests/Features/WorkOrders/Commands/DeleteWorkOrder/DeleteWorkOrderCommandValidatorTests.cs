using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.DeleteWorkOrder;

public class DeleteWorkOrderCommandValidatorTests
{
    private readonly DeleteWorkOrderCommandValidator _sut;

    public DeleteWorkOrderCommandValidatorTests()
    {
        _sut = new DeleteWorkOrderCommandValidator();
    }

    [Fact]
    public void DeleteWorkOrderCommandValidator_ShouldSucceed_WithValidInputs()
    {
        // Arrange
        var command = new DeleteWorkOrderCommand(Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DeleteWorkOrderCommandValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var command = new DeleteWorkOrderCommand(Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "id");
    }
}
