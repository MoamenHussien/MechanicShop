using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateOrderState;

public class UpdateWorkOrderStateCommandValidatorTests
{
    private readonly UpdateWorkOrderStateCommandValidator _sut;

    public UpdateWorkOrderStateCommandValidatorTests()
    {
        _sut = new UpdateWorkOrderStateCommandValidator();
    }

    [Fact]
    public void UpdateWorkOrderStateCommandValidator_ShouldSucceed_WithValidInputs()
    {
        // Arrange
        var command = new UpdateWorkOrderStateCommand(Guid.NewGuid(), WorkOrderState.InProgress);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void UpdateWorkOrderStateCommandValidator_ShouldFail_WithEmptyWorkOrderId()
    {
        // Arrange
        var command = new UpdateWorkOrderStateCommand(Guid.Empty, WorkOrderState.InProgress);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "WordOrderId");
    }

    [Fact]
    public void UpdateWorkOrderStateCommandValidator_ShouldFail_WithInvalidState()
    {
        // Arrange
        var command = new UpdateWorkOrderStateCommand(Guid.NewGuid(), (WorkOrderState)999);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "NewState");
    }
}
